using System;
using System.IO;
using System.Threading;
using Apache.NMS;
using Fudge;
using Fudge.Encodings;
using Fudge.Serialization;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    public class ClientResultStream<T> : DisposableBase
    {
        private readonly Action _stopAction;
        private readonly IConnection _connection;
        private readonly ISession _session;
        private readonly IDestination _destination;
        private readonly IMessageConsumer _consumer;
        private readonly MQTemplate _mqTemplate;
        private readonly OpenGammaFudgeContext _fudgeContext;

        public ClientResultStream(OpenGammaFudgeContext fudgeContext, MQTemplate mqTemplate, string topicName, Action stopAction)
        {
            _fudgeContext = fudgeContext;
            _stopAction = stopAction;


            _mqTemplate = mqTemplate;


            _connection = _mqTemplate.CreateConnection();
            _session = _connection.CreateSession();

            _destination = _session.GetDestination("topic://"+topicName);

            _consumer = _session.CreateConsumer(_destination);
            
            _connection.Start();
        }

        public T GetNext(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IMessage message = null;
            while (message== null)
            {
                message = _consumer.Receive(TimeSpan.FromSeconds(1));//TODO make this cancellable in a more sane manner
                cancellationToken.ThrowIfCancellationRequested();
            }
            

            cancellationToken.ThrowIfCancellationRequested();
            var bytesMessage = (IBytesMessage) message;
            using (var memoryStream = new MemoryStream(bytesMessage.Content))
            {
                var fudgeEncodedStreamReader = new FudgeEncodedStreamReader(_fudgeContext, memoryStream);
                return _fudgeContext.GetSerializer().Deserialize<T>(fudgeEncodedStreamReader);
            }
        }

        protected override void Dispose(bool disposing)
        {
            _consumer.Dispose();
            _session.Dispose();
            _connection.Dispose();


            _stopAction();
        }
    }
}