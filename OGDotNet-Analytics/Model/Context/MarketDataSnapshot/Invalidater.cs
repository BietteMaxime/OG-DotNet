//-----------------------------------------------------------------------
// <copyright file="Invalidater.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Threading;
using Apache.NMS.Util;
using OGDotNet.Mappedtypes;
using OGDotNet.Utils;

namespace OGDotNet.Model.Context.MarketDataSnapshot
{
    public abstract class Invalidater<T> : DisposableBase where T : class, IDisposable
    {
        private readonly AtomicReference<CancellationTokenSource> _currentTokenSource = new AtomicReference<CancellationTokenSource>(new CancellationTokenSource());

        private readonly object _currentLock = new object();
        private T _current;
        private Exception _currentException;

        protected Invalidater()
        {
            Invalidate();
        }

        public void Invalidate()
        {
            _currentTokenSource.Value.Cancel();
            ScheduleRebuild();
        }

        private void ScheduleRebuild()
        {
            ThreadPool.QueueUserWorkItem(Rebuild);
        }

        private void Rebuild(object state)
        {
            //TODO: cheaper waiting
            if (! _currentTokenSource.Value.IsCancellationRequested)
            {
                //Nothing to do
                return;
            }
            lock (_currentLock)
            {
                if (IsDisposed)
                {
                    return;
                }
                var newToken = new CancellationTokenSource();
                var oldToken = _currentTokenSource.GetAndSet(newToken);
                if (! oldToken.IsCancellationRequested)
                {
                    //Nothing to do
                    return;
                }

                var old = _current;
                Monitor.PulseAll(_currentLock);

                Dispose(old);

                try
                {
                    T newObject = Build(newToken.Token);
                    _current = newObject;
                    _currentException = null;
                }
                catch (Exception e)
                {
                    _current = null;
                    _currentException = e;
                }
            }
        }

        private static void Dispose(T old)
        {
            if (old != null)
            {
                old.Dispose();
            }
        }

        /// <summary>
        /// Will run the action on some built T.
        ///   If invalidate has been called before calling this method then the T will be rebuilt before the action is run.
        ///   If invalidate is called after calling this method T may or may not have been rebuilt before the action is called.
        /// </summary>
        /// <param name="ct">Throws if set</param>
        /// <param name="action">The action to run on the build t</param>
        public void With(CancellationToken ct, Action<T> action)
        {
            With(ct, _ =>
                     {
                         action(_);
                         return string.Empty;
                     });
        }

        public TRet With<TRet>(CancellationToken ct, Func<T, TRet> action)
        {
            lock (_currentLock)
            {
                while (true)
                {
                    ct.ThrowIfCancellationRequested();
                    CheckDisposed();
                    if (! _currentTokenSource.Value.IsCancellationRequested)
                    {
                        if (_currentException != null)
                        {
                            throw new OpenGammaException(_currentException.Message, _currentException);
                        }
                        return action(_current); //TODO give this some combined token
                    }

                    //TODO less stupid
                    while (! Monitor.Wait(_currentLock, TimeSpan.FromSeconds(1)))
                    {
                        ct.ThrowIfCancellationRequested();    
                    }
                }
            }
        }

        protected abstract T Build(CancellationToken ct);

        protected override void Dispose(bool disposing)
        {
            //Serialize
            lock (_currentLock)
            {
                Dispose(_current);
                _current = null;
            }
        }
    }
}