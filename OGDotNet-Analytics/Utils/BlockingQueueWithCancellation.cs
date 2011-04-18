//-----------------------------------------------------------------------
// <copyright file="BlockingQueueWithCancellation.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace OGDotNet.Utils
{
    class BlockingQueueWithCancellation<T> : DisposableBase, IEnumerable<T>
    {
        private readonly CancellationToken _token;
        private readonly Semaphore _semaphore = new Semaphore(0, int.MaxValue);

        /// <summary>
        /// I still use this ConcurrentQueue because it still needs a thread safe en/dequeue, it will just always succeed
        /// </summary>
        private readonly ConcurrentQueue<T> _innerQueue = new ConcurrentQueue<T>();

        public BlockingQueueWithCancellation(CancellationToken token)
        {
            _token = token;
        }

        public void Enqueue(T t)
        {
            _innerQueue.Enqueue(t);
            _semaphore.Release();
        }

        public T Dequeue()
        {
            WaitHandle.WaitAny(new[] { _semaphore, _token.WaitHandle });
            _token.ThrowIfCancellationRequested();

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

        public IEnumerator<T> GetEnumerator()
        {
            return EnumerateImpl().GetEnumerator();
        }

        private IEnumerable<T> EnumerateImpl()
        {
            while (true)
            {
                yield return Dequeue();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
