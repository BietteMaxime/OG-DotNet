using System;
using System.Threading;
using Xunit;
using Xunit.Sdk;
using TimeoutException = Xunit.Sdk.TimeoutException;

namespace OGDotNet.Tests.Integration.Xunit.Extensions
{
    internal class CustomizingCommand : DelegatingTestCommand
    {
        public CustomizingCommand(ITestCommand innerCommand)
            : base(innerCommand)
        {
        }

        private static int DefaultTimeout
        {
            get { return (int) TimeSpan.FromMinutes(5).TotalMilliseconds; }
        }

        public override MethodResult Execute(object testClass)
        {
            //We have to do timeout ourselves, because xunit can't handle the fact that it's own TimeOutCommand returns exceptions with null stack traces
            //It also leaves the method executing, which hangs the build.



            MethodResult innerResult = null;
            Exception innerEx = null;
            var thread = new Thread((() =>
                            {
                                try
                                {
                                    innerResult = InnerCommand.Execute(testClass);
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
                    throw new Exception("Exception occured in MUT",innerEx);
                }
                Assert.NotNull(innerResult);
                return innerResult;
            }
            else
            {
                thread.Abort();
                throw new TimeoutException(DefaultTimeout);
            }

        }
    }
}