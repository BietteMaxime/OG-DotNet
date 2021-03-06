﻿//-----------------------------------------------------------------------
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
using OGDotNet.Mappedtypes.Core.MarketDataSnapshot;
using OGDotNet.Mappedtypes.Engine.Value;
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Mappedtypes.Financial.User;
using OGDotNet.Mappedtypes.Financial.View;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Model.Context.MarketDataSnapshot
{
    public class SnapshotDataStreamInvalidater : Invalidater<SnapshotDataStream>
    {
        private readonly ManualResetEventSlim _constructedEvent = new ManualResetEventSlim(false);
        private readonly SnapshotLiveDataStreamInvalidater _liveStream;
        private readonly RemoteEngineContext _remoteEngineContext;
        private readonly UniqueId _snapshotId;
        private readonly FinancialClient _financialClient;

        public SnapshotDataStreamInvalidater(SnapshotLiveDataStreamInvalidater liveStream, RemoteEngineContext remoteEngineContext, UniqueId snapshotId)
        {
            _liveStream = liveStream;
            _remoteEngineContext = remoteEngineContext;
            _snapshotId = snapshotId;
            _financialClient = remoteEngineContext.CreateFinancialClient();
            _liveStream.GraphChanged += OnGraphChanged;
            
            _constructedEvent.Set();
        }

        protected override SnapshotDataStream Build(CancellationToken ct)
        {
            _constructedEvent.Wait(ct);

            ViewDefinition viewDefinition = null;
            Dictionary<YieldCurveKey, Dictionary<string, ValueRequirement>> specs = null;

            _liveStream.With(ct, liveDataStream => liveDataStream.WithLastResults(ct,
                                                                    (cycle, results) =>
                                                                        {
                                                                            var tempViewName = string.Format("{0}-{1}", typeof(SnapshotDataStream).Name, Guid.NewGuid());
                                                                            specs = _remoteEngineContext.ViewProcessor.MarketDataSnapshotter.GetYieldCurveRequirements(liveDataStream.RemoteViewClient, cycle);
                                                                            viewDefinition = GetViewDefinition(specs);
                                                                            viewDefinition.Name = tempViewName;
                                                                            var uid = _financialClient.ViewDefinitionRepository.
                                                                                AddViewDefinition(new AddViewDefinitionRequest(viewDefinition));
                                                                            viewDefinition.UniqueID = uid;
                                                                        }));
            return new SnapshotDataStream(viewDefinition, _remoteEngineContext, _snapshotId.ToLatest(), specs);
        }

        private void OnGraphChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private static ViewDefinition GetViewDefinition(Dictionary<YieldCurveKey, Dictionary<string, ValueRequirement>> specs)
        {
            var calculationConfigurationsByName = new Dictionary<string, ViewCalculationConfiguration>
                                                      {
                                                          {"Default", new ViewCalculationConfiguration("Default", specs.SelectMany(s => s.Value.Values), new Dictionary<string, HashSet<Tuple<string, ValueProperties>>>())}
                                                      };
            return new ViewDefinition(null, calculationConfigurationsByName: calculationConfigurationsByName);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _liveStream.GraphChanged -= OnGraphChanged;
                _financialClient.Dispose();
            }
        }
    }
}