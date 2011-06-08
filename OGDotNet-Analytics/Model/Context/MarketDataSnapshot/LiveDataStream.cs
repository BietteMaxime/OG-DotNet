//-----------------------------------------------------------------------
// <copyright file="LiveDataStream.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine.depGraph;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.calc;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Mappedtypes.engine.View.listener;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
using OGDotNet.Mappedtypes.Util.tuple;
using OGDotNet.Model.Resources;
using OGDotNet.Utils;

namespace OGDotNet.Model.Context.MarketDataSnapshot
{
    public class LiveDataStream : DisposableBase, INotifyPropertyChanged
    {
        private readonly string _basisViewName;
        private readonly RemoteEngineContext _remoteEngineContext;
        private readonly ManualResetEventSlim _prepared = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _haveLastResults = new ManualResetEventSlim(false);

        public event PropertyChangedEventHandler PropertyChanged;

        private RemoteViewClient _remoteViewClient;

        private volatile Dictionary<string, IDependencyGraph> _graphs;

        private readonly object  _lastResultsLock = new object();
        private Pair<Dictionary<string, IDependencyGraph>,
            Pair<IEngineResourceReference<IViewCycle>, IViewComputationResultModel>>
            _lastResults;

        public LiveDataStream(string basisViewName, RemoteEngineContext remoteEngineContext)
        {
            _basisViewName = basisViewName;
            _remoteEngineContext = remoteEngineContext;
            ThreadPool.QueueUserWorkItem(Prepare);
        }

        private void Prepare(object state)
        {
            try
            {
                _remoteViewClient = _remoteEngineContext.ViewProcessor.CreateClient();
                var eventViewResultListener = new EventViewResultListener();
                eventViewResultListener.CycleCompleted += (sender, e) => Update(e.FullResult);
                eventViewResultListener.ViewDefinitionCompiled += (sender, e) => { _graphs = null; };
                _remoteViewClient.SetResultListener(eventViewResultListener);
                _remoteViewClient.SetViewCycleAccessSupported(true);
                _remoteViewClient.AttachToViewProcess(_basisViewName, ExecutionOptions.RealTime);
            }
            finally
            {
                _prepared.Set();
            }
        }

        [IndexerName("Item")]
        public double? this[MarketDataValueSpecification spec, string valueName]
        {
            get
            {
                var lastResults = _lastResults;
                //TODO index this
                var result = lastResults.Second.Second.AllLiveData.FirstOrDefault(
                        d =>
                        d.Specification.TargetSpecification.Uid == spec.UniqueId &&
                        d.Specification.ValueName == valueName);
                if (result == null)
                {
                    return null;
                }
                return (double?)result.Value;
            }
        }

        private void Update(IViewComputationResultModel results)
        {
            UpdateLastResults(results);
            InvokePropertyChanged(new PropertyChangedEventArgs("Item[]"));
        }

        private void UpdateLastResults(IViewComputationResultModel results)
        {
            lock (_lastResultsLock)
            {
                IEngineResourceReference<IViewCycle> resourceReference =
                    _remoteViewClient.CreateCycleReference(results.ViewCycleId);

                if (_graphs == null)
                {
                    _graphs = RawMarketDataSnapper.GetGraphs(resourceReference.Value.GetCompiledViewDefinition());
                }
                var previous = Interlocked.Exchange(ref _lastResults,
                                                    Pair.Create(_graphs, Pair.Create(resourceReference, results)));
                if (previous != null)
                {
                    _haveLastResults.Set(); // TODO : this is a hack for PLAT-1325
                    previous.Second.First.Dispose();
                }
            }
        }

        private void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _prepared.Wait();
                if (_remoteViewClient != null)
                {
                    _remoteViewClient.Dispose();
                }
            }
        }

        public ManageableMarketDataSnapshot GetNewSnapshotForUpdate(CancellationToken ct = default(CancellationToken))
        {
            _haveLastResults.Wait(ct);
            lock (_lastResultsLock)
            {
                IViewComputationResultModel results = _lastResults.Second.Second;
                return RawMarketDataSnapper.CreateSnapshotFromCycle(results,
                                                                    _lastResults.First, _lastResults.Second.First.Value,
                                                                    _basisViewName);
            }
        }
    }
}
