//-----------------------------------------------------------------------
// <copyright file="LastResultViewClient.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine.depGraph;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.calc;
using OGDotNet.Mappedtypes.engine.View.listener;
using OGDotNet.Mappedtypes.Util.tuple;
using OGDotNet.Model.Resources;
using OGDotNet.Utils;

namespace OGDotNet.Model.Context.MarketDataSnapshot
{
    public abstract class LastResultViewClient : DisposableBase, INotifyPropertyChanged
    {
        public event EventHandler GraphChanged;

        private readonly RemoteEngineContext _remoteEngineContext;
        private readonly ManualResetEventSlim _prepared = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _haveLastResults = new ManualResetEventSlim(false);

        public event PropertyChangedEventHandler PropertyChanged;

        private RemoteViewClient _remoteViewClient;

        private volatile Dictionary<string, IDependencyGraph> _graphs;

        private readonly object _lastResultsLock = new object();
        private Pair<Dictionary<string, IDependencyGraph>,
            Pair<IEngineResourceReference<IViewCycle>, IViewComputationResultModel>>
            _lastResults;

        protected LastResultViewClient(RemoteEngineContext remoteEngineContext)
        {
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
                AttachToViewProcess(_remoteViewClient);
            }
            finally
            {
                _prepared.Set();
            }
        }

        protected RemoteViewClient RemoteViewClient
        {
            get { return _remoteViewClient; }
        }

        protected abstract void AttachToViewProcess(RemoteViewClient remoteViewClient);

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
                    InvokeGraphChanged();
                }

                var previous = Interlocked.Exchange(ref _lastResults,
                                                    Pair.Create(_graphs, Pair.Create(resourceReference, results)));
                bool ready = (!ShouldWaitForExtraCycle) || previous != null;
                if (ready)
                {
                    _haveLastResults.Set(); 
                }
                if (previous != null)
                {
                    previous.Second.First.Dispose();
                }
            }
        }

        /// <Reremarks>
        /// TODO : this is a hack for PLAT-1325
        /// </Reremarks>
        protected abstract bool ShouldWaitForExtraCycle { get; }

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

        public T WithLastResults<T>(CancellationToken ct, Func<IViewCycle, IDictionary<string, IDependencyGraph>, IViewComputationResultModel, T> func)
        {
            _haveLastResults.Wait(ct);
            lock (_lastResultsLock)
            {
                return func(_lastResults.Second.First.Value, _lastResults.First, _lastResults.Second.Second);
            }
        }

        protected T WithLastResults<T>(Func<IViewCycle, IDictionary<string, IDependencyGraph>, IViewComputationResultModel, bool> waitFor, CancellationToken ct, Func<IViewCycle, IDictionary<string, IDependencyGraph>, IViewComputationResultModel, T> func)
        {
            WaitFor(waitFor, ct);
            return WithLastResults(ct, func);
        }

        private void WaitFor(Func<IViewCycle, IDictionary<string, IDependencyGraph>, IViewComputationResultModel, bool> waitFor, CancellationToken ct)
        {
             _haveLastResults.Wait(ct);
            using (var mre = new ManualResetEventSlim())
            {
                PropertyChangedEventHandler onPropChanged = delegate
                                                                {
                                                                    mre.Set();
                                                                };
                PropertyChanged += onPropChanged;
                try
                {
                    while (true)
                    {
                        mre.Reset();
                        if (WithLastResults(ct, waitFor))
                        {
                            return;
                        }
                        mre.Wait(ct);
                    }
                }
                finally
                {
                    PropertyChanged -= onPropChanged;
                }
            }
        }

        private void InvokeGraphChanged()
        {
            EventHandler handler = GraphChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}