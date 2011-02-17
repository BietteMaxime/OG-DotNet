using System;
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
            if (! TryExecuteWithTimeout(work, out ret))
            {
                throw new TimeoutException(DefaultTimeout);
            }
            return ret;
        }

        /// <returns>False iff the command timed out</returns>
        /// <exception cref="Exception">If the inner function threw an exception</exception>
        internal static bool TryExecuteWithTimeout<T>(Func<T> work, out T ret)
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
            thread.Start();
            if (thread.Join(DefaultTimeout))
            {
                if (innerEx != null)
                {
                    ExceptionUtility.RethrowWithNoStackTraceLoss(innerEx);
                }
                ret =innerResult;
                return true;
            }
            else
            {
                thread.Abort();
                ret = default(T);
                return false;
            }
        }
    }
}