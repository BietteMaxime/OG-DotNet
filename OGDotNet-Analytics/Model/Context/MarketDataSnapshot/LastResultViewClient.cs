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
using System.Threading.Tasks;
using OGDotNet.Mappedtypes;
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
        private readonly Task _prepared;
        private readonly ManualResetEventSlim _haveLastResults = new ManualResetEventSlim(false);

        public event PropertyChangedEventHandler PropertyChanged;

        private RemoteViewClient _remoteViewClient;

        private volatile Dictionary<string, IDependencyGraph> _graphs;

        private volatile JavaException _error;

        private readonly object _lastResultsLock = new object();
        private Pair<Dictionary<string, IDependencyGraph>,
            Pair<IEngineResourceReference<IViewCycle>, IViewComputationResultModel>>
            _lastResults;

        protected LastResultViewClient(RemoteEngineContext remoteEngineContext)
        {
            _remoteEngineContext = remoteEngineContext;
            _prepared = new Task(Prepare);
            _prepared.Start();
        }

        private void Prepare()
        {
            _remoteViewClient = _remoteEngineContext.ViewProcessor.CreateClient();
            var eventViewResultListener = new EventViewResultListener();
            eventViewResultListener.CycleCompleted += (sender, e) => Update(e.FullResult);
            eventViewResultListener.ViewDefinitionCompiled += (sender, e) => { _graphs = null; };

            eventViewResultListener.ViewDefinitionCompilationFailed += (sender, e) =>
                                                                           {
                                                                               _error = e.Exception;
                                                                               _haveLastResults.Set();
                                                                           };
            eventViewResultListener.CycleExecutionFailed += (sender, e) =>
            {
                _error = e.Exception;
                _haveLastResults.Set();
            };
            _remoteViewClient.SetResultListener(eventViewResultListener);
            _remoteViewClient.SetViewCycleAccessSupported(true);
            AttachToViewProcess(_remoteViewClient);
        }

        protected RemoteEngineContext RemoteEngineContext
        {
            get { return _remoteEngineContext; }
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
                
                if (resourceReference == null)
                {
                    //The engine has overtaken us.  We'll get another one soon
                    // -- In theory this can live lock if the view is very fast
                    return;
                }
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
                else
                {
                    ThreadPool.RegisterWaitForSingleObject(_haveLastResults.WaitHandle, delegate
                                                            {
                                                                //Give up on the hack LAP-60
                                                                _haveLastResults.Set();
                                                            }, null,
                                                           TimeSpan.FromSeconds(10), true);
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

        public void WithLastResults(CancellationToken ct, Action<IViewCycle, IDictionary<string, IDependencyGraph>, IViewComputationResultModel> action)
        {
            WithLastResults<object>(ct, (c, g, m) =>
                                            {
                                                action(c, g, m);
                                                return null;
                                            });
        }
        public T WithLastResults<T>(CancellationToken ct, Func<IViewCycle, IDictionary<string, IDependencyGraph>, IViewComputationResultModel, T> func)
        {
            _prepared.Wait(ct);
            WaitForAResult(ct);
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
            _prepared.Wait(ct);
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

        private void WaitForAResult(CancellationToken ct)
        {
            _haveLastResults.Wait(ct);
            if (_error != null)
            {
                throw new OpenGammaException("Failed to get results", _error.BuildException());
            }
        }

        private void InvokeGraphChanged()
        {
            EventHandler handler = GraphChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}