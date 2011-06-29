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
using OGDotNet.Mappedtypes;
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
using OGDotNet.Utils;

namespace OGDotNet.Model.Context.MarketDataSnapshot
{
    public class SnapshotDataStream : LastResultViewClient
    {
        private readonly string _basisViewName;
        private readonly UniqueIdentifier _snapshotId;
        private readonly LiveDataStream _liveDataStream;
        private readonly RemoteClient _remoteClient;

        private readonly AutoResetEvent _recompileEvent = new AutoResetEvent(true);
        private readonly ManualResetEvent _viewDefinitionCreated = new ManualResetEvent(true);
        private readonly object _attachLock = new object();

        private readonly string _tempviewName;
        private volatile UniqueIdentifier _tempViewUid;
        private volatile Exception _error;

        public SnapshotDataStream(string basisViewName, RemoteEngineContext remoteEngineContext, UniqueIdentifier snapshotId, LiveDataStream liveDataStream) : base(remoteEngineContext)
        {
            _basisViewName = basisViewName;
            _snapshotId = snapshotId;
            _liveDataStream = liveDataStream;

            _remoteClient = remoteEngineContext.CreateUserClient();

            _tempviewName = string.Format("{0}-{1}-{2}", typeof(SnapshotDataStream).Name, _basisViewName, Guid.NewGuid());
            _remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(new ViewDefinition(_tempviewName))); //Make sure we have something to attach to
            _viewDefinitionCreated.Set();

            _liveDataStream.GraphChanged += LiveDataStreamGraphChanged;
            _liveDataStream.BasisViewNameChanged += LiveDataStreamBasisViewNameChanged;

            ThreadPool.RegisterWaitForSingleObject(_recompileEvent, Recompile, null, -1, false);
        }

        void LiveDataStreamGraphChanged(object sender, EventArgs e)
        {
            _recompileEvent.Set();
        }

        private void LiveDataStreamBasisViewNameChanged(object sender, EventArgs e)
        {
            _tempViewUid = GetNewUid();
            _error = null;
        }

        private void Recompile(object state, bool timedout)
        {
            ArgumentChecker.Not(timedout, "timedOut");
            try
            {
                IgnoreDisposingExceptions(() =>
                {
                    _tempViewUid = GetNewUid();
                    _liveDataStream.WithLastResults(default(CancellationToken),
                            (cycle, graphs, results) =>
                                {
                                    if (IsDisposed)
                                    {
                                        return; // double check here since we might have waited for a long time
                                    }
                                    _remoteClient.ViewDefinitionRepository.UpdateViewDefinition(new UpdateViewDefinitionRequest(_tempviewName, GetViewDefinition(graphs, _tempViewUid)));
                                });

                    AttachToViewProcess(RemoteViewClient); //TODO: should happen magically
                });
            }
            catch (Exception e)
            {
                _error = e;
            }
        }

        private void IgnoreDisposingExceptions(Action a)
        {
            if (IsDisposed)
            {
                return;
            }
            try
            {
                a();
            }
            catch (DataNotFoundException)
            {
                if (IsDisposed)
                {
                    return;
                }

                throw;
            }
            catch (InvalidOperationException)
            {
                if (IsDisposed)
                {
                    return;
                }

                throw;
            }
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
            if (IsDisposed)
            {
                return;
            }
            _viewDefinitionCreated.WaitOne();
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
            if (_error != null)
            {
                throw new OpenGammaException("Failed to build snapshot view", _error);
            }
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
                _liveDataStream.BasisViewNameChanged -= LiveDataStreamBasisViewNameChanged;
            }
            base.Dispose(disposing);
            if (disposing)
            {
                _remoteClient.Dispose();
                _recompileEvent.Dispose();
                _remoteClient.ViewDefinitionRepository.RemoveViewDefinition(_tempviewName);
                _viewDefinitionCreated.Dispose();
            }
        }
    }
}