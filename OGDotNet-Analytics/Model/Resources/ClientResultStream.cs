//-----------------------------------------------------------------------
// <copyright file="ClientResultStream.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using Apache.NMS;
using Fudge.Encodings;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    public class ClientResultStream<T> : DisposableBase
    {
        private readonly IConnection _connection;
        private readonly ISession _session;
        private readonly ITemporaryQueue _destination;
        private readonly IMessageConsumer _consumer;
        private readonly MQTemplate _mqTemplate;
        private readonly OpenGammaFudgeContext _fudgeContext;

        public event EventHandler<ResultEvent> MessageReceived;

        public ClientResultStream(OpenGammaFudgeContext fudgeContext, MQTemplate mqTemplate)
        {
            _fudgeContext = fudgeContext;

            _mqTemplate = mqTemplate;

            _connection = _mqTemplate.CreateConnection();
            _session = _connection.CreateSession();

            _destination = _session.CreateTemporaryQueue();

            _consumer = _session.CreateConsumer(_destination);

            _consumer.Listener += msg => InvokeMessageReceived(new ResultEvent(Deserialize(msg)));

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

        private T Deserialize(IMessage message)
        {
            //TODO check SEQ numbers without making this 20% slower
            byte[] content = ((IBytesMessage)message).Content;
            using (var memoryStream = new MemoryStream(content))
            {
                var fudgeEncodedStreamReader = new FudgeEncodedStreamReader(_fudgeContext, memoryStream);
                return _fudgeContext.GetSerializer().Deserialize<T>(fudgeEncodedStreamReader);
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