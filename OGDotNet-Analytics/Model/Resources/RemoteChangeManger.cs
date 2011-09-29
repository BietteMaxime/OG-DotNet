//-----------------------------------------------------------------------
// <copyright file="RemoteChangeManger.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Apache.NMS;
using OGDotNet.Mappedtypes.Core.Change;

namespace OGDotNet.Model.Resources
{
    public class RemoteChangeManger
    {
        private readonly RestTarget _restTarget;
        private readonly string _activeMQSpec;

        private readonly object _listenersLock = new object();
        private readonly HashSet<IChangeListener> _listeners = new HashSet<IChangeListener>();
        private readonly FudgeMessageDecoder _fudgeDecoder;
        private IConnection _connection;
        
        public RemoteChangeManger(RestTarget restTarget, string activeMQSpec, OpenGammaFudgeContext fudgeContext)
        {
            _restTarget = restTarget;
            _activeMQSpec = activeMQSpec;
            _fudgeDecoder = new FudgeMessageDecoder(fudgeContext, false);
        }

        public void AddChangeListener(IChangeListener aggregatingChangeListener)
        {
            lock (_listenersLock)
            {
                if (!_listeners.Add(aggregatingChangeListener))
                {
                    return;
                }
                if (_listeners.Count == 1)
                {
                    Connect();
                }
            }
        }

        public void RemoveChangeListener(IChangeListener aggregatingChangeListener)
        {
            lock (_listenersLock)
            {
                if (!_listeners.Remove(aggregatingChangeListener))
                {
                    return;
                }
                if (_listeners.Count == 0)
                {
                    Disconnect();
                }
            }
        }

        private void Connect()
        {
            var fudgeMsg = _restTarget.Resolve("topicName").GetFudge();
            var topicName = fudgeMsg.GetString("value");
            _connection = new MQTemplate(_activeMQSpec).CreateConnection();
            _connection.Start();
            var session = _connection.CreateSession();
            var topic = session.GetTopic(topicName);
            var messageConsumer = session.CreateConsumer(topic);
            messageConsumer.ConsumerTransformer = _fudgeDecoder.FudgeDecodeMessage;
            messageConsumer.Listener += MessageReceived;
        }

        private void Disconnect()
        {
            _connection.Dispose();
        }

        private void MessageReceived(IMessage message)
        {
            var changeEvent = (ChangeEvent)((IObjectMessage)message).Body;
            List<IChangeListener> listeners;
            lock (_listenersLock)
            {
                listeners = _listeners.ToList();
            }
            foreach (var changeListener in listeners)
            {
                changeListener.EntityChanged(changeEvent);
            }
        }
    }
}