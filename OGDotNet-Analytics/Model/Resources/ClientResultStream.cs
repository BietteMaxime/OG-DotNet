//-----------------------------------------------------------------------
// <copyright file="ClientResultStream.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Apache.NMS;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    public class ClientResultStream<T> : DisposableBase
    {
        private readonly MQTemplate _mqTemplate;
        private readonly FudgeMessageDecoder _fudgeMessageDecoder;

        private readonly IConnection _connection;
        private readonly ISession _session;
        private readonly ITemporaryQueue _destination;
        private readonly IMessageConsumer _consumer;
        
        public event EventHandler<ResultEvent> MessageReceived;

        public ClientResultStream(OpenGammaFudgeContext fudgeContext, MQTemplate mqTemplate)
        {
            _mqTemplate = mqTemplate;

            _fudgeMessageDecoder = new FudgeMessageDecoder(fudgeContext);
            _connection = _mqTemplate.CreateConnection();
            _session = _connection.CreateSession();

            _destination = _session.CreateTemporaryQueue();

            _consumer = _session.CreateConsumer(_destination);

            _consumer.ConsumerTransformer = _fudgeMessageDecoder.FudgeDecodeMessage;
            _consumer.Listener += msg => InvokeMessageReceived(new ResultEvent(((IObjectMessage) msg).Body));

            _connection.Start();
        }

        public string QueueName
        {
            get
            {
                CheckDisposed();
                return _destination.QueueName;
            }
        }

        private void InvokeMessageReceived(ResultEvent e)
        {
            EventHandler<ResultEvent> handler = MessageReceived;
            if (handler != null) handler(this, e);
        }

        protected override void Dispose(bool disposing)
        {
            _consumer.Dispose();
            _session.Dispose();
            _connection.Dispose();
        }
    }
}