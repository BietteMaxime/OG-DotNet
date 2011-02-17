using System;
using System.Diagnostics;
using System.Threading;
using Xunit.Sdk;
using TimeoutException = Xunit.Sdk.TimeoutException;

namespace OGDotNet.Tests.Integration.Xunit.Extensions
{
    internal static class ManualTimeout
    {
        private static int DefaultTimeout
        {
            get { return (int) TimeSpan.FromMinutes(5).TotalMilliseconds; }
        }

        internal static T ExecuteWithTimeout<T>(Func<T> work)
        {
            T ret;
            StackTrace timedOutTrace;
            if (! TryExecuteWithTimeout(work, out ret, out timedOutTrace))
            {
                throw ExceptionUtility.GetExceptionWithStackTrace(new TimeoutException(DefaultTimeout), timedOutTrace);
            }
            return ret;
        }

        /// <returns>False iff the command timed out</returns>
        /// <exception cref="Exception">If the inner function threw an exception</exception>
        internal static bool TryExecuteWithTimeout<T>(Func<T> work, out T ret, out StackTrace timedOutTrace)
        {
            T innerResult = default(T);
            Exception innerEx = null;
            var thread = new Thread((() =>
                                {
                                    try
                                    {
                                        innerResult = work();
                                    }
                                    catch (Exception e)
                                    {
                                        innerEx = e;
                                    }
                                }));
            thread.IsBackground = true;
            thread.Start();
            if (thread.Join(DefaultTimeout))
            {
                if (innerEx != null)
                {

                    ExceptionUtility.RethrowWithNoStackTraceLoss(innerEx);
                }
                ret =innerResult;
                timedOutTrace = null;
                return true;
            }
            else
            {
#pragma warning disable 612,618
                thread.Suspend();
                timedOutTrace = new StackTrace(thread, false);
                thread.Resume();
#pragma warning restore 612,618


                thread.Abort();
                ret = default(T);
                return false;
            }
        }
    }
}