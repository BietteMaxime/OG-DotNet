//-----------------------------------------------------------------------
// <copyright file="RestfulJmsResultConsumerBase.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    public abstract class RestfulJmsResultConsumerBase<TListener> : DisposableBase where TListener : class //TODO IObservable
    {
        private class ListenerReference
        {
            //DOTNET-24 : This makes sure that the ClientResultStream doesn't reference the RestfulJmsResultConsumerBase
            public readonly object ListenerLock = new object();
            private readonly Action<object, TListener> _action;
            public TListener ResultListener;

            public ListenerReference(Action<object, TListener> action)
            {
                _action = action;
            }

            public void ListenerResultReceived(object sender, MsgEvent result)
            {
                lock (ListenerLock)
                {
                    TListener viewResultListener = ResultListener;

                    if (viewResultListener == null)
                    {
                        return;
                    }
                    _action(result.Msg, viewResultListener); // Needs to be done in the lock to maintain order
                }
            }
        }

        private readonly ListenerReference _listenerReference;
        protected readonly OpenGammaFudgeContext FudgeContext;
        private readonly MQTemplate _mqTemplate;
        protected readonly RestTarget REST;
        private readonly HeartbeatSender _heartbeatSender;

        private ClientResultStream _listenerResultStream;

        protected RestfulJmsResultConsumerBase(OpenGammaFudgeContext fudgeContext, RestTarget clientUri, MQTemplate mqTemplate, Action<object, TListener> resultAction)
        {
            FudgeContext = fudgeContext;
            _mqTemplate = mqTemplate;
            REST = clientUri;

            _listenerReference = new ListenerReference(resultAction); //TODO: check that this doesn't reference us
            _heartbeatSender = new HeartbeatSender(TimeSpan.FromSeconds(10), REST.Resolve("heartbeat"));
        }

        public void SetResultListener(TListener resultListener)
        {
            lock (_listenerReference.ListenerLock)
            {
                if (_listenerReference.ResultListener != null)
                {
                    throw new InvalidOperationException("Result listener already set");
                }
                // NOTE: exception throwing call first
                _listenerResultStream = StartResultStream();
                _listenerResultStream.MessageReceived += _listenerReference.ListenerResultReceived;
                _listenerReference.ResultListener = resultListener;
            }
        }

        public void RemoveResultListener()
        {
            RemoveResultListenerInner(true);
        }

        private void RemoveResultListenerInner(bool throwOnNotSet)
        {
            ClientResultStream listenerResultStream;
            lock (_listenerReference.ListenerLock)
            {
                if (_listenerReference.ResultListener == null)
                {
                    if (throwOnNotSet)
                    {
                        throw new InvalidOperationException("Result listener not currently set");
                    }
                    return;
                }
                _listenerReference.ResultListener = null;

                listenerResultStream = _listenerResultStream;
                _listenerResultStream = null;
                StopResultStream();
            }
            listenerResultStream.Dispose(); // If this is done in the lock then dispose can deadlock
        }

        private ClientResultStream StartResultStream()
        {
            var clientResultStream = new ClientResultStream(FudgeContext, _mqTemplate);
            try
            {
                REST.Resolve("startJmsResultStream").PostFudge(new FudgeMsg(FudgeContext) { { "destination", clientResultStream.QueueName } });
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
            REST.Resolve("endJmsResultStream").Post();
        }

        public void Shutdown()
        {
            REST.Delete();
        }

        protected override void Dispose(bool disposing)
        {
            _heartbeatSender.Dispose();
            IgnoreDisposingExceptions(() => RemoveResultListenerInner(false));
            IgnoreDisposingExceptions(Shutdown);
        }
    }
}