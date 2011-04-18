//-----------------------------------------------------------------------
// <copyright file="BlockingQueueWithCancellation.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace OGDotNet.Utils
{
    class BlockingQueueWithCancellation<T> : DisposableBase
    {
        private readonly Semaphore _semaphore = new Semaphore(0, int.MaxValue);

        /// <summary>
        /// I still use this ConcurrentQueue because it still needs a thread safe en/dequeue, it will just always succeed
        /// </summary>
        private readonly ConcurrentQueue<T> _innerQueue = new ConcurrentQueue<T>();
        
        public void Enqueue(T t)
        {
            _innerQueue.Enqueue(t);
            _semaphore.Release();
        }

        public T Dequeue(CancellationToken cancellationToken)
        {
            WaitHandle.WaitAny(new[] { _semaphore, cancellationToken.WaitHandle });
            cancellationToken.ThrowIfCancellationRequested();

            T t;
            if (!_innerQueue.TryDequeue(out t))
            {
                throw new ArgumentException();
            }
            return t;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {//BlockingCollection seems to not bother disposing its wait handles (via CancellationToken), but I'm too scared
                _semaphore.Dispose();
            }
        }
    }
}
