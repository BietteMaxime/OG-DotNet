//-----------------------------------------------------------------------
// <copyright file="SnapshotDataStreamInvalidater.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OGDotNet.Mappedtypes.Engine.DepGraph;
using OGDotNet.Mappedtypes.Engine.Value;
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Mappedtypes.Financial.View;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Model.Resources;

namespace OGDotNet.Model.Context.MarketDataSnapshot
{
    public class SnapshotDataStreamInvalidater : Invalidater<SnapshotDataStream>
    {
        private readonly ManualResetEventSlim _constructedEvent = new ManualResetEventSlim(false);
        private readonly SnapshotLiveDataStreamInvalidater _liveStream;
        private readonly RemoteEngineContext _remoteEngineContext;
        private readonly UniqueId _snapshotId;
        private readonly RemoteClient _remoteClient;

        public SnapshotDataStreamInvalidater(SnapshotLiveDataStreamInvalidater liveStream, RemoteEngineContext remoteEngineContext, UniqueId snapshotId)
        {
            _liveStream = liveStream;
            _remoteEngineContext = remoteEngineContext;
            _snapshotId = snapshotId;
            _remoteClient = remoteEngineContext.CreateUserClient();
            _liveStream.GraphChanged += OnGraphChanged;
            
            _constructedEvent.Set();
        }

        protected override SnapshotDataStream Build(CancellationToken ct)
        {
            _constructedEvent.Wait(ct);

            ViewDefinition viewDefinition = null;
            _liveStream.With(ct, liveDataStream => liveDataStream.WithLastResults(ct,
                                                                    (cycle, graphs, results) =>
                                                                        {
                                                                            var tempViewName = string.Format("{0}-{1}", typeof(SnapshotDataStream).Name, Guid.NewGuid());
                                                                            viewDefinition = GetViewDefinition(tempViewName, graphs);
                                                                            viewDefinition.Name = tempViewName;
                                                                            _remoteClient.ViewDefinitionRepository.
                                                                                AddViewDefinition(new AddViewDefinitionRequest(viewDefinition));
                                                                        }));
            return new SnapshotDataStream(viewDefinition, _remoteEngineContext, _snapshotId.ToLatest());
        }

        private void OnGraphChanged(object sender, EventArgs e)
        {
            Invalidate();
        }
        private static ViewDefinition GetViewDefinition(string name, IDictionary<string, IDependencyGraph> graphs)
        {
            IEnumerable<ValueSpecification> specs = RawMarketDataSnapper.GetYieldCurveSpecs(graphs);
            var calculationConfigurationsByName = new Dictionary<string, ViewCalculationConfiguration>
                                                      {
                                                          {"Default", new ViewCalculationConfiguration("Default", specs.Select(ToRequirement), new Dictionary<string, HashSet<Tuple<string, ValueProperties>>>())}
                                                      };
            return new ViewDefinition(name, calculationConfigurationsByName: calculationConfigurationsByName);
        }

        private static ValueRequirement ToRequirement(ValueSpecification spec)
        {
            return new ValueRequirement(spec.ValueName, spec.TargetSpecification, spec.Properties);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _liveStream.GraphChanged -= OnGraphChanged;
                _remoteClient.Dispose();
            }
        }
    }
}