//-----------------------------------------------------------------------
// <copyright file="RemoteClient.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    public class RemoteClient : DisposableBase
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
            ThreadPool.RegisterWaitForSingleObject(cancellationToken.WaitHandle, SendHeartBeats, cancellationToken, TimeSpan.FromMinutes(5), true);
        }

        private void SendHeartBeats(object context, bool timedOut)
        {
            var token = (CancellationToken)context;
            if (token.IsCancellationRequested)
            {
                _cts.Dispose();
            }
            else
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

        public InterpolatedYieldCurveDefinitionMaster InterpolatedYieldCurveDefinitionMaster
        {
            get
            {
                return new InterpolatedYieldCurveDefinitionMaster(_rest.Resolve("interpolatedYieldCurveDefinitions"));
            }
        }

        public RemoteManagableViewDefinitionRepository ViewDefinitionRepository
        {
            get
            {
                return new RemoteManagableViewDefinitionRepository(_rest);
            }
        }

        protected override void Dispose(bool disposing)
        {
            _cts.Cancel();
            //NOTE: I can't dispose of _cts here, because then calling WaitHandle on it would throw
        }
    }
}