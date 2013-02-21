// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SnapshotDataStreamInvalidater.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using OpenGamma.Core.Config.Impl;
using OpenGamma.Engine.Value;
using OpenGamma.Engine.View;
using OpenGamma.Financial.User;
using OpenGamma.Id;
using OpenGamma.MarketDataSnapshot;
using OpenGamma.Master.Config;

namespace OpenGamma.Model.Context.MarketDataSnapshot
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
                                                                            var configItem = ConfigItem.Create(viewDefinition, viewDefinition.Name);
                                                                            var doc = new ConfigDocument<ViewDefinition>(configItem);
                                                                            doc = _financialClient.ConfigMaster.Add(doc);
                                                                            viewDefinition.UniqueId = doc.UniqueId;
                                                                        }));
            return new SnapshotDataStream(viewDefinition, _remoteEngineContext, _snapshotId.ToLatest(), specs);
        }

        private void OnGraphChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private static ViewDefinition GetViewDefinition(Dictionary<YieldCurveKey, Dictionary<string, ValueRequirement>> specs)
        {
            var calcConfig = new ViewCalculationConfiguration("Default");
            calcConfig.AddSpecificRequirements(specs.SelectMany(s => s.Value.Values));
            var vd = new ViewDefinition(null);
            vd.AddCalculationConfiguration(calcConfig);
            return vd;
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