using System;
using System.Threading;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    public class RemoteClient:DisposableBase
    {
        private readonly string _clientId;
        private readonly RestTarget _rest;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public RemoteClient(RestTarget userDataRest)
            : this(userDataRest, Environment.UserName, Guid.NewGuid().ToString())
        {
        }

        private RemoteClient(RestTarget userDataRest, string username, string clientId)
        {
            _clientId = clientId;
            _rest = userDataRest.Resolve(username).Resolve("clients").Resolve(_clientId);

            QueueHeartbeat(_cts.Token);

        }

        private void QueueHeartbeat(CancellationToken cancellationToken)
        {
            ThreadPool.RegisterWaitForSingleObject(cancellationToken.WaitHandle, SendHeartBeats, cancellationToken, TimeSpan.FromSeconds(5), true);
        }

        private  void SendHeartBeats(object context, bool timedOut)
        {
            var token = (CancellationToken) context;
            if (! token.IsCancellationRequested)
            {
                try
                {
                    SendHeartbeat();
                }
                finally
                {
                    QueueHeartbeat(token);
                }
            }

        }

        private void SendHeartbeat()
        {
            _rest.Resolve("heartbeat").Post(); 
        }

        public RemoteSecurityMaster SecurityMaster
        {
            get
            {
                return new RemoteSecurityMaster(_rest.Resolve("securities"));
            }
        }

        protected override void Dispose(bool disposing)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}