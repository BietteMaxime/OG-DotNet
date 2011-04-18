//-----------------------------------------------------------------------
// <copyright file="RemoteViewClient.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.engine.View;
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

        private readonly object _listenerLock = new object();
        private IViewResultListener _resultListener;
        private ClientResultStream<object> _listenerResultStream;

        public RemoteViewClient(OpenGammaFudgeContext fudgeContext, RestTarget clientUri, MQTemplate mqTemplate)
        {
            _fudgeContext = fudgeContext;
            _mqTemplate = mqTemplate;
            _rest = clientUri;
        }
        
        public void SetResultListener(IViewResultListener resultListener)
        {
            lock (_listenerLock)
            {
                if (_resultListener != null)
                {
                    throw new InvalidOperationException("Result listener already set");
                }
                _resultListener = resultListener;
                _listenerResultStream = StartResultStream();
                _listenerResultStream.MessageReceived += ListenerResultReceived;
            }
        }

        private void ListenerResultReceived(object sender, ResultEvent e)
        {
            IViewResultListener viewResultListener;
            lock (_listenerLock)
            {
                viewResultListener = _resultListener;
            }
            if (viewResultListener == null)
            {
                return;
            }
            e.ApplyTo(viewResultListener); // If this is done in the lock then dispose can deadlock
        }

        public void RemoveResultListener()
        {
            RemoveResultListenerInner(true);
        }

        private void RemoveResultListenerInner(bool throwOnNotSet)
        {
            ClientResultStream<object> listenerResultStream;
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

        private void Shutdown()
        {
            _rest.Resolve("shutdown").Post();
        }

        private ClientResultStream<object> StartResultStream()
        {
            var reponse = _rest.Resolve("startJmsResultStream").Post();
            return new ClientResultStream<object>(_fudgeContext, _mqTemplate, reponse.GetValue<string>("value"));
        }

        private void StopResultStream()
        {
            _rest.Resolve("endJmsResultStream").Post();
        }

        public void SetUpdatePeriod(long periodMillis)
        {
            _rest.Resolve("updatePeriod").Post<object>(periodMillis, "updatePeriod");
        }

        public bool IsAttached
        {
            get
            {
                var reponse = _rest.Resolve("isAttached").GetFudge();
                return 1 == (sbyte)reponse.GetByName("value").Value;
            }
        }

        public void AttachToViewProcess(string viewDefinitionName, IViewExecutionOptions executionOptions)
        {
            AttachToViewProcess(viewDefinitionName, executionOptions, false);
        }

        public void AttachToViewProcess(string viewDefinitionName, IViewExecutionOptions executionOptions, bool newBatchProcess)
        {
            AttachToViewProcessRequest request = new AttachToViewProcessRequest(viewDefinitionName, executionOptions, newBatchProcess);
            var reponse = _rest.Resolve("attachSearch").Post(request);
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

        public InMemoryViewComputationResultModel GetLatestResult()
        {
            return _rest.Resolve("latestResult").Get<InMemoryViewComputationResultModel>();
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
            RemoveResultListenerInner(false);
            Shutdown();
        }
    }
}