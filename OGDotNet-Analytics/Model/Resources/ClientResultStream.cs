//-----------------------------------------------------------------------
// <copyright file="ClientResultStream.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using Apache.NMS;
using Fudge.Encodings;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.listener;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    public class ClientResultStream : DisposableBase
    {
        private readonly Action _stopAction;
        private readonly IConnection _connection;
        private readonly ISession _session;
        private readonly IDestination _destination;
        private readonly IMessageConsumer _consumer;
        private readonly MQTemplate _mqTemplate;
        private readonly OpenGammaFudgeContext _fudgeContext;

        readonly BlockingQueueWithCancellation<IMessage> _messageQueue = new BlockingQueueWithCancellation<IMessage>();

        public ClientResultStream(OpenGammaFudgeContext fudgeContext, MQTemplate mqTemplate, string topicName, Action stopAction)
        {
            _fudgeContext = fudgeContext;
            _stopAction = stopAction;

            _mqTemplate = mqTemplate;

            _connection = _mqTemplate.CreateConnection();
            _session = _connection.CreateSession();

            _destination = _session.GetDestination("topic://" + topicName);

            _consumer = _session.CreateConsumer(_destination);

            _consumer.Listener += msg => _messageQueue.Enqueue(msg);

            _connection.Start();
        }

        public InMemoryViewComputationResultModel GetNext(CancellationToken cancellationToken)
        {
            InMemoryViewComputationResultModel ret = null;
            while (ret == null)
            {
                cancellationToken.ThrowIfCancellationRequested();

                IMessage message = _messageQueue.TryDequeue(cancellationToken);

                ret = Deserialize(message);
            }
            return ret;
        }

        private InMemoryViewComputationResultModel Deserialize(IMessage message)
        {
            var bytesMessage = (IBytesMessage)message;
            using (var memoryStream = new MemoryStream(bytesMessage.Content))
            {
                var fudgeEncodedStreamReader = new FudgeEncodedStreamReader(_fudgeContext, memoryStream);
                var ret = _fudgeContext.GetSerializer().Deserialize(fudgeEncodedStreamReader);

                //TODO less hideous
                if (ret is CycleCompletedCall)
                {
                    return ((CycleCompletedCall)ret).FullResult;
                }
                else if (ret is ViewDefinitionCompiledCall)
                {
                    return null;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            _consumer.Dispose();
            _session.Dispose();
            _connection.Dispose();

            _messageQueue.Dispose();

            _stopAction();
        }
    }
}