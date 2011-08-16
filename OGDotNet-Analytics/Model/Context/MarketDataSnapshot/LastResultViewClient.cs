//-----------------------------------------------------------------------
// <copyright file="LastResultViewClient.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OGDotNet.Mappedtypes;
using OGDotNet.Mappedtypes.Engine.DepGraph;
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Mappedtypes.Engine.View.Calc;
using OGDotNet.Mappedtypes.Engine.View.Compilation;
using OGDotNet.Mappedtypes.Engine.View.Listener;
using OGDotNet.Mappedtypes.Util.Tuple;
using OGDotNet.Model.Resources;
using OGDotNet.Utils;

namespace OGDotNet.Model.Context.MarketDataSnapshot
{
    public abstract class LastResultViewClient : DisposableBase
    {
        public event EventHandler GraphChanged;

        private readonly RemoteEngineContext _remoteEngineContext;
        private readonly Task _prepared;
        private readonly ManualResetEventSlim _haveResults = new ManualResetEventSlim(false);

        private readonly object _lastResultsLock = new object();
        private Pair<Dictionary<string, IDependencyGraph>,
            Pair<IEngineResourceReference<IViewCycle>, IViewComputationResultModel>>
            _lastResults;

        private RemoteViewClient _remoteViewClient;

        private bool _graphsOutOfDate;
        private Dictionary<string, IDependencyGraph> _graphs;

        private volatile Exception _error; //TODO: combine multiple exceptions

        protected LastResultViewClient(RemoteEngineContext remoteEngineContext)
        {
            _remoteEngineContext = remoteEngineContext;
            _prepared = new Task(Prepare);
            _prepared.Start();
        }

        private void Prepare()
        {
            try
            {
                CheckDisposed();
                _remoteViewClient = _remoteEngineContext.ViewProcessor.CreateClient();
                CheckDisposed();

                var eventViewResultListener = new EventViewResultListener();
                eventViewResultListener.CycleCompleted += (sender, e) => Update(e.FullResult);
                eventViewResultListener.ViewDefinitionCompilationFailed += (sender, e) => SetError(e.Exception);
                eventViewResultListener.ViewDefinitionCompiled += delegate
                                                                      {
                                                                          _graphsOutOfDate = true;
                                                                      };
                eventViewResultListener.CycleExecutionFailed += (sender, e) => SetError(e.Exception);
                _remoteViewClient.SetResultListener(eventViewResultListener);
                _remoteViewClient.SetViewCycleAccessSupported(true);

                CheckDisposed();
                AttachToViewProcess(_remoteViewClient);
            }
            catch (Exception ex)
            {
                SetError(ex);
            }
        }

        public RemoteViewClient RemoteViewClient
        {
            get { return _remoteViewClient; }
        }

        private void SetError(JavaException javaException)
        {
            Exception buildException = javaException.BuildException();
            SetError(buildException);
        }

        private void SetError(Exception buildException)
        {
            //TODO combine errors
            _error = buildException;
            _haveResults.Set();
            InvokeGraphChanged();
        }

        protected RemoteEngineContext RemoteEngineContext
        {
            get { return _remoteEngineContext; }
        }

        protected abstract void AttachToViewProcess(RemoteViewClient remoteViewClient);

        private void Update(IViewComputationResultModel results)
        {
            lock (_lastResultsLock)
            {
                CheckDisposed();
                Monitor.PulseAll(_lastResultsLock);

                IEngineResourceReference<IViewCycle> resourceReference =
                    _remoteViewClient.CreateCycleReference(results.ViewCycleId);

                if (resourceReference == null)
                {
                    //The engine has overtaken us.  We'll get another one soon
                    // -- In theory this can live lock if the view is very fast
                    return;
                }
                if (_graphsOutOfDate)
                {
                    _graphsOutOfDate = false; //NOTE: this is safe because our result message are serialized with our compiled notifications
                    ICompiledViewDefinitionWithGraphs compiledViewDefinitionWithGraphs = resourceReference.Value.GetCompiledViewDefinition();
                    _graphs = compiledViewDefinitionWithGraphs.CompiledCalculationConfigurations.Keys
                        .ToDictionary(k => k, k => compiledViewDefinitionWithGraphs.GetDependencyGraphExplorer(k).GetWholeGraph());
                    InvokeGraphChanged();
                }

                var newResults = Pair.Create(_graphs, Pair.Create(resourceReference, results));
                var previous = Interlocked.Exchange(ref _lastResults, newResults);

                _haveResults.Set();

                if (previous != null)
                {
                    previous.Second.First.Dispose();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _prepared.Wait();
                lock (_lastResultsLock)
                {
                    var lastResults = _lastResults;
                    _lastResults = null;
                    if (lastResults != null)
                    {
                        //NOTE: the engine would clean this up anyway
                        lastResults.Second.First.Dispose();
                    }
                    Monitor.PulseAll(_lastResultsLock);
                }

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

        protected T WithLastResults<T>(CancellationToken ct, Func<IViewCycle, IDictionary<string, IDependencyGraph>, IViewComputationResultModel, T> func)
        {
            WaitForAResult(ct);
            lock (_lastResultsLock)
            {
                CheckDisposed();
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
            WaitForAResult(ct);
            lock (_lastResultsLock)
            {
                while (true)
                {
                    CheckDisposed();
                    if (WithLastResults(ct, waitFor))
                    {
                        return;
                    }
                    Monitor.Wait(_lastResultsLock);
                }
            }
        }

        private void WaitForAResult(CancellationToken ct)
        {
            _prepared.Wait(ct); //Make sure we see any errors here
            _haveResults.Wait(ct);
            if (_error != null)
            {
                throw new OpenGammaException("Failed to get results", _error);
            }
        }

        private void InvokeGraphChanged()
        {
            EventHandler handler = GraphChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}