//-----------------------------------------------------------------------
// <copyright file="SnapshotDataStream.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine.depGraph;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.calc;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Mappedtypes.financial.analytics.ircurve;
using OGDotNet.Mappedtypes.financial.model.interestrate.curve;
using OGDotNet.Mappedtypes.financial.view;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.math.curve;
using OGDotNet.Mappedtypes.util.PublicAPI;
using OGDotNet.Model.Resources;

namespace OGDotNet.Model.Context.MarketDataSnapshot
{
    public class SnapshotDataStream : LastResultViewClient
    {
        private readonly string _basisViewName;
        private readonly RemoteEngineContext _remoteEngineContext;
        private readonly UniqueIdentifier _snapshotId;
        private readonly LiveDataStream _liveDataStream;
        private readonly RemoteClient _remoteClient;

        private readonly AutoResetEvent _recompileEvent = new AutoResetEvent(true);
        private readonly object _attachLock = new object();

        private readonly string _tempviewName;
        private volatile UniqueIdentifier _tempViewUid;

        public SnapshotDataStream(string basisViewName, RemoteEngineContext remoteEngineContext, UniqueIdentifier snapshotId, LiveDataStream liveDataStream) : base(remoteEngineContext)
        {
            _basisViewName = basisViewName;
            _remoteEngineContext = remoteEngineContext;
            _snapshotId = snapshotId;
            _liveDataStream = liveDataStream;

            _remoteClient = remoteEngineContext.CreateUserClient();
            _liveDataStream.GraphChanged += LiveDataStreamGraphChanged;

            _tempviewName = string.Format("{0}-{1}-{2}", typeof(SnapshotDataStream).Name, _basisViewName, Guid.NewGuid());
            _remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(new ViewDefinition(_tempviewName))); //Make sure we have something to attach to

            ThreadPool.RegisterWaitForSingleObject(_recompileEvent, Recompile, null, int.MaxValue, false);
        }

        void LiveDataStreamGraphChanged(object sender, EventArgs e)
        {
            _recompileEvent.Set();
        }

        private void Recompile(object state, bool timedout)
        {
            if (timedout)
            {
                return;
            }

            if (IsDisposed)
            {
                return;
            }

            _tempViewUid = GetNewUid();
            _liveDataStream.WithLastResults(default(CancellationToken), (cycle, graphs, results) =>
                                                                            {
                                                                                _remoteClient.ViewDefinitionRepository.UpdateViewDefinition(new UpdateViewDefinitionRequest(_tempviewName, GetViewDefinition(graphs, _tempViewUid)));
                                                                                ViewDefinition viewDefinition = _remoteEngineContext.ViewProcessor.ViewDefinitionRepository.GetViewDefinition(_tempviewName);
                                                                                return viewDefinition;
                                                                            });
            
            AttachToViewProcess(RemoteViewClient); //TODO: should happen magically
        }

        private ViewDefinition GetViewDefinition(IDictionary<string, IDependencyGraph> graphs, UniqueIdentifier uid)
        {
            IEnumerable<ValueSpecification> specs = RawMarketDataSnapper.GetYieldCurveSpecs(graphs);
            var calculationConfigurationsByName = new Dictionary<string, ViewCalculationConfiguration>
                                                      {
                                                          {"Default", new ViewCalculationConfiguration("Default", specs.Select(ToRequirement), new Dictionary<string, HashSet<Tuple<string, ValueProperties>>>())}
                                                      };
            return new ViewDefinition(_tempviewName, uniqueID: uid, calculationConfigurationsByName: calculationConfigurationsByName);
        }

        private UniqueIdentifier GetNewUid()
        {
            return UniqueIdentifier.Of("Defn", _tempviewName, Guid.NewGuid().ToString());
        }

        private static ValueRequirement ToRequirement(ValueSpecification spec)
        {
            return new ValueRequirement(spec.ValueName, spec.TargetSpecification, spec.Properties);
        }

        protected override void AttachToViewProcess(RemoteViewClient remoteViewClient)
        {
            lock (_attachLock)
            {
                if (remoteViewClient == null)
                {
                    return;
                }
                if (remoteViewClient.GetState() == ViewClientState.Terminated)
                {
                    return;
                }
                if (remoteViewClient.IsAttached)
                {
                    RemoteViewClient.DetachFromViewProcess();
                }
                remoteViewClient.AttachToViewProcess(_tempviewName, ExecutionOptions.Snapshot(_snapshotId));
            }
        }

        public Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities, NodalDoublesCurve>> GetYieldCurves(DateTimeOffset waitFor, CancellationToken ct)
        {
            return WithLastResults((cycle, graphs, results) => Matches(results, waitFor, cycle), 
                ct, (cycle, graphs, results) => RawMarketDataSnapper.EvaluateYieldCurves(results));
        }

        private bool Matches(IViewComputationResultModel results, DateTimeOffset waitFor, IViewCycle cycle)
        {
            var viewDefinition = cycle.GetCompiledViewDefinition().ViewDefinition;
            UniqueIdentifier uniqueIdentifier = viewDefinition.UniqueID;
            if (results.ValuationTime < waitFor)
            {
                return false;
            }
            
            if (_tempViewUid == null || uniqueIdentifier != _tempViewUid)
            {
                return false;
            }
            return true;
        }

        protected override bool ShouldWaitForExtraCycle
        {
            get { return false; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _liveDataStream.GraphChanged -= LiveDataStreamGraphChanged;
            }
            base.Dispose(disposing);
            if (disposing)
            {
                _remoteClient.Dispose();
                _recompileEvent.Dispose();
                _remoteClient.ViewDefinitionRepository.RemoveViewDefinition(_tempviewName);
            }
        }
    }
}