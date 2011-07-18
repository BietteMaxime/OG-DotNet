//-----------------------------------------------------------------------
// <copyright file="DisposableBase.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using OGDotNet.Mappedtypes;

namespace OGDotNet.Utils
{
    public abstract class DisposableBase : IDisposable
    {
        private readonly object _disposingLock = new object();
        private bool _disposed = false;
        ~DisposableBase()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            lock (_disposingLock)
            {
                if (_disposed)
                    return;
                _disposed = true;
            }
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void CheckDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(string.Format("Cannot access disposed {0}", GetType().Name));
            }
        }
        protected bool IsDisposed
        {
            get { return _disposed; }
        }

        protected abstract void Dispose(bool disposing);

        protected void IgnoreDisposingExceptions(Action a)
        {
            try
            {
                a();
            }
            catch (DataNotFoundException)
            {
                if (IsDisposed)
                {
                    return;
                }

                throw;
            }
            catch (InvalidOperationException)
            {
                if (IsDisposed)
                {
                    return;
                }

                throw;
            }
        }
    }
}