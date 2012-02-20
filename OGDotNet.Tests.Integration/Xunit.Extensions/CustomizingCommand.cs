//-----------------------------------------------------------------------
// <copyright file="CustomizingCommand.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit.Sdk;

namespace OGDotNet.Tests.Integration.Xunit.Extensions
{
    internal class CustomizingCommand : DelegatingTestCommand
    {
        public CustomizingCommand(ITestCommand innerCommand)
            : base(innerCommand)
        {
        }

        public override MethodResult Execute(object testClass)
        {
            /*
             * We have to do timeout ourselves, because xunit can't handle the fact that it's own TimeOutCommand returns exceptions with null stack traces
             * It also leaves the method executing, which hangs the build.
             */

            return WithRetry(3, 6,
                delegate
                {
                    try
                    {
                        return ManualTimeout.ExecuteWithTimeout(() => InnerCommand.Execute(testClass));
                    }
                    finally
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                });
        }

        private static T WithRetry<T>(int shortAttempts, int maxAttempts, Func<T> action)
        {
            if (Debugger.IsAttached)
            {
                return action();
            }
            var es = new List<Exception>();
            int succcess = 0;
            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    var result = action();
                    if (i == 0)
                    {
                        return result;
                    }
                    else
                    {
                        succcess++;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Potentially retrying after " + e);
                    if (i == shortAttempts - 1 && succcess == 0)
                    {
                        //No success, presume it's just failing
                        throw;
                    }
                    es.Add(e);
                }
            }
            var message = string.Format("Intermittent failure {0}/{1} times: {2}", maxAttempts - succcess, maxAttempts, string.Join(",", es.Select(e => e.Message).Distinct()));
            throw new AggregateException(message, es);
        }
    }
}