//-----------------------------------------------------------------------
// <copyright file="RemoteChangeManger.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Apache.NMS;
using OGDotNet.Mappedtypes.Core.Change;

namespace OGDotNet.Model.Resources
{
    public class RemoteChangeManger
    {
        private readonly Lazy<MQTopicTemplate> _topicTemplate;

        private readonly object _listenersLock = new object();
        private readonly HashSet<IChangeListener> _listeners = new HashSet<IChangeListener>();
        private readonly FudgeMessageDecoder _fudgeDecoder;
        private IConnection _connection;

        public RemoteChangeManger(RestTarget restTarget, OpenGammaFudgeContext fudgeContext)
        {
            _topicTemplate = new Lazy<MQTopicTemplate>(() => GetTopicTemplate(restTarget));
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
            var mqTopicTemplate = _topicTemplate.Value;
            _connection = mqTopicTemplate.Template.CreateConnection();
            _connection.Start();
            var session = _connection.CreateSession();
            var topic = session.GetTopic(mqTopicTemplate.TopicName);
            var messageConsumer = session.CreateConsumer(topic);
            messageConsumer.ConsumerTransformer = _fudgeDecoder.FudgeDecodeMessage;
            messageConsumer.Listener += MessageReceived;
        }

        private static MQTopicTemplate GetTopicTemplate(RestTarget restTarget)
        {
            var specFudge = restTarget.Resolve("brokerUri").GetFudge();
            var activeMQSpec = specFudge.GetString("value");
            return GetTopicTemplate(activeMQSpec, restTarget);
        }

        private static MQTopicTemplate GetTopicTemplate(string activeMQSpec, RestTarget restTarget)
        {
            var fudgeMsg = restTarget.Resolve("topicName").GetFudge();
            var topicName = fudgeMsg.GetString("value");
            var template = new MQTemplate(activeMQSpec);
            return new MQTopicTemplate(template, topicName);
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