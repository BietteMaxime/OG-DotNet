//-----------------------------------------------------------------------
// <copyright file="DisposableBase.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace OGDotNet.Utils
{
    public abstract class DisposableBase : IDisposable
    {
        ~DisposableBase()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected abstract void Dispose(bool disposing);
    }
}