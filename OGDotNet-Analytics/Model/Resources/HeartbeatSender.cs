//-----------------------------------------------------------------------
// <copyright file="HeartbeatSender.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
    internal class HeartbeatSender : DisposableBase
    {
        private readonly CancellationTokenSource _heartbeatCancellationTokenSource = new CancellationTokenSource();
        private readonly TimeSpan _period;
        private readonly RestTarget _heartbeatRest;

        public HeartbeatSender(TimeSpan period, RestTarget heartbeatRest)
        {
            QueueHeartbeat(_heartbeatCancellationTokenSource.Token);
            _period = period;
            _heartbeatRest = heartbeatRest;
        }

        private void QueueHeartbeat(CancellationToken cancellationToken)
        {
            ThreadPool.RegisterWaitForSingleObject(cancellationToken.WaitHandle, SendHeartBeats, cancellationToken, _period, true);
        }

        private void SendHeartBeats(object context, bool timedOut)
        {
            var token = (CancellationToken)context;
            if (token.IsCancellationRequested)
            {
                _heartbeatCancellationTokenSource.Dispose();
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
            _heartbeatRest.Post();
        }

        protected override void Dispose(bool disposing)
        {
            _heartbeatCancellationTokenSource.Cancel();
            //NOTE: I can't dispose of _heartbeatCancellationTokenSource here, because then calling WaitHandle on it would throw
        }
    }
}