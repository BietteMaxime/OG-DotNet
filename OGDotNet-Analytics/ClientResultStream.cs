using System;
using System.IO;
using System.Threading;
using Apache.NMS;
using Fudge.Encodings;

namespace OGDotNet_Analytics
{
    public class ClientResultStream : DisposableBase
    {
        private readonly Uri _queueUri;
        private readonly string _queueName;
        private readonly Action _stopAction;
        private readonly IConnection _connection;
        private readonly ISession _session;
        private readonly IDestination _destination;
        private readonly IMessageConsumer _consumer;

        public ClientResultStream(Uri queueUri, string queueName, Action stopAction)
        {
            _queueUri = queueUri;
            _queueName = queueName;
            _stopAction = stopAction;


            var oldSkooluri = _queueUri.LocalPath.Replace("(", "").Replace(")", "");
            IConnectionFactory factory = new NMSConnectionFactory(oldSkooluri);

            _connection = factory.CreateConnection();
            _session = _connection.CreateSession();

            _destination = _session.GetDestination("topic://"+queueName); //?

            _consumer = _session.CreateConsumer(_destination);

            _connection.Start();
        }

        public ViewComputationResultModel GetNext(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var message = _consumer.Receive();
            var bytesMessage = message as IBytesMessage;
            using (var memoryStream = new MemoryStream(bytesMessage.Content))
            {
                var fudgeEncodedStreamReader = new FudgeEncodedStreamReader(FudgeConfig.GetFudgeContext(), memoryStream);
                return FudgeConfig.GetFudgeSerializer().Deserialize<ViewComputationResultModel>(fudgeEncodedStreamReader);
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