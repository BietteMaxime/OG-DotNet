//-----------------------------------------------------------------------
// <copyright file="ClientResultStream.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright Â© 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Apache.NMS;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    public class ClientResultStream : DisposableBase
    {
        private readonly MQTemplate _mqTemplate;
        private readonly FudgeMessageDecoder _fudgeMessageDecoder;

        private readonly IConnection _connection;
        private readonly ISession _session;
        private readonly ITemporaryQueue _destination;
        private readonly IMessageConsumer _consumer;

        public event EventHandler<MsgEvent> MessageReceived;

        public ClientResultStream(OpenGammaFudgeContext fudgeContext, MQTemplate mqTemplate)
            : this(fudgeContext, mqTemplate, true)
        {
        }

        public ClientResultStream(OpenGammaFudgeContext fudgeContext, MQTemplate mqTemplate, bool checkSeqNumber)
        {
            _mqTemplate = mqTemplate;

            _fudgeMessageDecoder = new FudgeMessageDecoder(fudgeContext, checkSeqNumber);
            _connection = _mqTemplate.CreateConnection();
            _session = _connection.CreateSession();

            _destination = _session.CreateTemporaryQueue();

            _consumer = _session.CreateConsumer(_destination);

            _consumer.ConsumerTransformer = _fudgeMessageDecoder.FudgeDecodeMessage;
            _consumer.Listener += msg => InvokeMessageReceived(((IObjectMessage) msg).Body);

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

        private void InvokeMessageReceived(object msg)
        {
            EventHandler<MsgEvent> handler = MessageReceived;
            if (handler != null) handler(this, new MsgEvent(msg));
        }

        protected override void Dispose(bool disposing)
        {
            _consumer.Dispose();
            _session.Dispose();
            _connection.Dispose();
        }
    }
}