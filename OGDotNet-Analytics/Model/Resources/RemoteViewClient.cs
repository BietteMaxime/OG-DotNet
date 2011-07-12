//-----------------------------------------------------------------------
// <copyright file="RemoteViewClient.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.calc;
using OGDotNet.Mappedtypes.engine.View.client;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Mappedtypes.engine.View.listener;
using OGDotNet.Mappedtypes.financial.view.rest;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.util.PublicAPI;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    /// <summary>
    /// See DataViewClientResource on the java side
    /// </summary>
    public class RemoteViewClient : DisposableBase  //TODO IObservable
    {
        private readonly OpenGammaFudgeContext _fudgeContext;
        private readonly MQTemplate _mqTemplate;
        private readonly RestTarget _rest;
        private readonly HeartbeatSender _heartbeatSender;

        private readonly object _listenerLock = new object();
        private IViewResultListener _resultListener;
        private ClientResultStream _listenerResultStream;

        public RemoteViewClient(OpenGammaFudgeContext fudgeContext, RestTarget clientUri, MQTemplate mqTemplate)
        {
            _fudgeContext = fudgeContext;
            _mqTemplate = mqTemplate;
            _rest = clientUri;
            _heartbeatSender = new HeartbeatSender(TimeSpan.FromSeconds(10), _rest.Resolve("heartbeat"));
        }
        
        public void SetResultListener(IViewResultListener resultListener)
        {
            lock (_listenerLock)
            {
                if (_resultListener != null)
                {
                    throw new InvalidOperationException("Result listener already set");
                }
                // NOTE: exception throwing call first
                _listenerResultStream = StartResultStream();
                _listenerResultStream.MessageReceived += ListenerResultReceived;
                _resultListener = resultListener;
            }
        }

        private void ListenerResultReceived(object sender, ResultEvent e)
        {
            lock (_listenerLock)
            {
                IViewResultListener viewResultListener = _resultListener;

                if (viewResultListener == null)
                {
                    return;
                }
                e.ApplyTo(viewResultListener); // Needs to be done in the lock to maintain order
            }
        }

        public void RemoveResultListener()
        {
            RemoveResultListenerInner(true);
        }

        private void RemoveResultListenerInner(bool throwOnNotSet)
        {
            ClientResultStream listenerResultStream;
            lock (_listenerLock)
            {
                if (_resultListener == null)
                {
                    if (throwOnNotSet)
                    {
                        throw new InvalidOperationException("Result listener not currently set");
                    }
                    return;
                }
                _resultListener = null;

                listenerResultStream = _listenerResultStream;
                _listenerResultStream = null;
                StopResultStream();
            }
            listenerResultStream.Dispose(); // If this is done in the lock then dispose can deadlock
        }
        public void Pause()
        {
            _rest.Resolve("pause").Post();
        }

        public void Resume()
        {
            _rest.Resolve("resume").Post();
        }

        public void TriggerCycle()
        {
            _rest.Resolve("triggerCycle").Post();
        }

        private void Shutdown()
        {
            _rest.Resolve("shutdown").Post();
        }

        private ClientResultStream StartResultStream()
        {
            var clientResultStream = new ClientResultStream(_fudgeContext, _mqTemplate);
            try
            {
                _rest.Resolve("startJmsResultStream").PostFudge(new FudgeMsg {{"destination", clientResultStream.QueueName}});
                return clientResultStream;    
            }
            catch
            {
                clientResultStream.Dispose();
                throw;
            }
        }

        private void StopResultStream()
        {
            _rest.Resolve("endJmsResultStream").Post();
        }

        public void SetUpdatePeriod(long periodMillis)
        {
            _rest.Resolve("updatePeriod").Put(new FudgeMsg(new Field("updatePeriod", periodMillis)));
        }

        public void SetViewResultMode(ViewResultMode mode)
        {
            var fudgeMsg = new FudgeMsg {{1, EnumBuilder<ViewResultMode>.GetJavaName(mode)}};
            _rest.Resolve("resultMode").Put(fudgeMsg);
        }

        public bool IsAttached
        {
            get
            {
                var reponse = _rest.Resolve("isAttached").GetFudge();
                return 1 == (sbyte)reponse.GetByName("value").Value;
            }
        }

        public void AttachToViewProcess(string viewDefinitionName, IViewExecutionOptions executionOptions, bool newBatchProcess = false)
        {
            ArgumentChecker.NotNull(viewDefinitionName, "viewDefinitionName");
            ArgumentChecker.NotNull(executionOptions, "executionOptions");
            AttachToViewProcessRequest request = new AttachToViewProcessRequest(viewDefinitionName, executionOptions, newBatchProcess);
            _rest.Resolve("attachSearch").Post(request);
        }
        public void DetachFromViewProcess()
        {
            _rest.Resolve("detach").Post();
        }

        public RemoteLiveDataInjector LiveDataOverrideInjector
        {
            get
            {
                return new RemoteLiveDataInjector(_rest.Resolve("overrides"));
            }
        }

        public bool IsResultAvailable
        {
            get
            {
                var reponse = _rest.Resolve("resultAvailable").GetFudge();
                return 1 == (sbyte)reponse.GetByName("value").Value;
            }
        }

        public bool IsCompleted
        {
            get
            {
                var reponse = _rest.Resolve("completed").GetFudge();
                return 1 == (sbyte)reponse.GetByName("value").Value;
            }
        }

        public bool GetViewCycleAccessSupported()
        {
            var reponse = _rest.Resolve("viewCycleAccessSupported").GetFudge();
            return 1 == (sbyte)reponse.GetByName("value").Value;
        }
        public void SetViewCycleAccessSupported(bool isViewCycleAccessSupported)
        {
            var msg = _fudgeContext.NewMessage(
                new Field("isViewCycleAccessSupported", isViewCycleAccessSupported)
            );
            _rest.Resolve("viewCycleAccessSupported").PostFudge(msg);
        }

        public IEngineResourceReference<IViewCycle> CreateLatestCycleReference()
        {
            return CreateCycleReference(null);
        }

        public IEngineResourceReference<IViewCycle> CreateCycleReference(UniqueIdentifier cycleId)
        {
            var location = _rest.Resolve("createLatestCycleReference").Create(cycleId);
            return location == null ? null : new RemoteViewCycleReference(location);
        }

        public IViewComputationResultModel GetLatestResult()
        {
            return _rest.Resolve("latestResult").Get<IViewComputationResultModel>();
        }

         public ViewDefinition GetViewDefinition()
         {
             return _rest.Resolve("viewDefinition").Get<ViewDefinition>();
         }

        public UniqueIdentifier GetUniqueId()
        {
            var restTarget = _rest.Resolve("id");
            return restTarget.Get<UniqueIdentifier>();
        }
        public ViewClientState GetState()
        {
            var target = _rest.Resolve("state");
            var msg = target.GetFudge();

            return EnumBuilder<ViewClientState>.Parse((string) msg.GetByOrdinal(1).Value);
        }

        protected override void Dispose(bool disposing)
        {
            _heartbeatSender.Dispose();
            IgnoreDisposingExceptions(delegate
            {
                RemoveResultListenerInner(false);
                Shutdown();
            });
        }
    }
}