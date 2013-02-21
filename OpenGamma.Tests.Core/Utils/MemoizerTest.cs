// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MemoizerTest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

using OpenGamma.Util;

using Xunit;

namespace OpenGamma.Utils
{
    public class MemoizerTest
    {
        [Fact]
        public void ConcurrentAlwaysAddsReturnSingelton()
        {
            const int repeats = 10;
            for (int i = 0; i < repeats; i++)
            {
                ConcurrentAddsReturnSingelton();
            }

            Parallel.For(1, repeats, i => ConcurrentAddsReturnSingelton());
        }

        [Fact]
        public void ConcurrentAddsReturnSingelton()
        {
            var wait = TimeSpan.FromMinutes(1);

            var b = new Barrier(2);
            long functionCount = 0;
            var m = new Memoizer<int, object>(i =>
                                          {
                                              var count = Interlocked.Increment(ref functionCount);
                                              if (count > 2)
                                              {
                                                  throw new Exception("Too many executions " + count);
                                              }

                                              Assert.True(b.SignalAndWait(wait));
                                              return new object();
                                          });
            const int arg = 1;
            var o1 = Task<object>.Factory.StartNew(() => m.Get(arg));
            var o2 = Task<object>.Factory.StartNew(() => m.Get(arg));
            Assert.True(o1.Wait(wait));
            Assert.True(o2.Wait(wait));
            Assert.Equal(2, functionCount);
            Assert.Equal(o1.Result, o2.Result);
        }
    }
}
