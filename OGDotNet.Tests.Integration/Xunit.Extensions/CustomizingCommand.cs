//-----------------------------------------------------------------------
// <copyright file="CustomizingCommand.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
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

            try
            {
                return ManualTimeout.ExecuteWithTimeout(() => InnerCommand.Execute(testClass));
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}