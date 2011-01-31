using System;
using System.IO;
using System.Threading;
using Apache.NMS;
using Fudge.Encodings;

namespace OGDotNet_Analytics
{
    public class ClientResultStream<T> : DisposableBase //TODO IObservable
    {
        private readonly Uri _serviceUri;
        private readonly string _topicName;
        private readonly Action _stopAction;
        private readonly IConnection _connection;
        private readonly ISession _session;
        private readonly IDestination _destination;
        private readonly IMessageConsumer _consumer;

        public ClientResultStream(Uri serviceUri, string topicName, Action stopAction)
        {
            _serviceUri = serviceUri;
            _topicName = topicName;
            _stopAction = stopAction;


            var oldSkooluri = _serviceUri.LocalPath.Replace("(", "").Replace(")", "");
            IConnectionFactory factory = new NMSConnectionFactory(oldSkooluri);

            _connection = factory.CreateConnection();
            _session = _connection.CreateSession();

            _destination = _session.GetDestination("topic://"+topicName); //?

            _consumer = _session.CreateConsumer(_destination);

            _connection.Start();
        }

        public T GetNext(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var message = _consumer.Receive();
            var bytesMessage = message as IBytesMessage;
            using (var memoryStream = new MemoryStream(bytesMessage.Content))
            {
                var fudgeEncodedStreamReader = new FudgeEncodedStreamReader(FudgeConfig.GetFudgeContext(), memoryStream);
                return FudgeConfig.GetFudgeSerializer().Deserialize<T>(fudgeEncodedStreamReader);
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