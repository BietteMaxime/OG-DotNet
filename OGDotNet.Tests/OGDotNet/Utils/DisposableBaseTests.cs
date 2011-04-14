//-----------------------------------------------------------------------
// <copyright file="DisposableBaseTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using OGDotNet.Utils;
using Xunit;

namespace OGDotNet.Tests.OGDotNet.Utils
{
    public class DisposableBaseTests
    {
        [Fact]
        public void ManualDispose_CalledExactlyOnce()
        {
            var r = CountDisposedCalls(d => d.Dispose());

            Assert.Equal(1, r.Item1);
            Assert.Equal(0, r.Item2);
        }

        [Fact]
        public void Finalize_CalledExactlyOnce()
        {
            var r = CountDisposedCalls(d => { });

            Assert.Equal(0, r.Item1);
            Assert.Equal(1, r.Item2);
        }

        private static Tuple<long, long> CountDisposedCalls(Action<IDisposable> action)
        {
            long manual = 0;
            long auto = 0;
            DoAction(action, delegate(bool disposing)
                                  {
                                      if (disposing)
                                          Interlocked.Increment(ref manual);
                                      else
                                          Interlocked.Increment(ref auto);
                                  });
            GC.Collect();
            GC.WaitForFullGCComplete();
            GC.WaitForPendingFinalizers();

            return new Tuple<long, long>(Interlocked.Read(ref manual), Interlocked.Read(ref auto));
        }

        /// <summary>
        /// NOTE: making this a separate method lets the GC do it's job
        /// </summary>
        private static void DoAction(Action<IDisposable> action, Action<bool> innerAction)
        {
            var obj = new DisposableBaseImpl(innerAction);
            action(obj);
        }

        private class DisposableBaseImpl : DisposableBase
        {
            private readonly Action<bool> _onDispose;

            public DisposableBaseImpl(Action<bool> onDispose)
            {
                _onDispose = onDispose;
            }

            protected override void Dispose(bool disposing)
            {
                _onDispose(disposing);
            }
        }
    }
}
