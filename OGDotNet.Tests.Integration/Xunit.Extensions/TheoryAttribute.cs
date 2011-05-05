//-----------------------------------------------------------------------
// <copyright file="TheoryAttribute.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit.Sdk;
using TimeoutException = System.TimeoutException;

namespace OGDotNet.Tests.Integration.Xunit.Extensions
{
    public class TheoryAttribute : global::Xunit.Extensions.TheoryAttribute
    {
        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            IEnumerable<ITestCommand> testCommands;
            StackTrace timedOutTrace;
            bool succeded = ManualTimeout.TryExecuteWithTimeout(() => BaseEnumerateTestCommands(method), out testCommands, out timedOutTrace);
            if (!succeded)
            {
                return ParameterGenerationTimedOutCommand.GetSingleCommand(method, timedOutTrace);
            }

            IEnumerable<CustomizingCommand> serialCommands = testCommands.Select(cmd => new CustomizingCommand(cmd));
            var repeatedCommands = Enumerable.Repeat(serialCommands, Repeat).SelectMany(s => s);
            return (!Debugger.IsAttached && Parallel) ? ParallelCommandGroup.WrapGroup(repeatedCommands) : repeatedCommands;
        }

        private bool _parallel = true;
        public bool Parallel
        {
            get { return _parallel; }
            set { _parallel = value; }
        }

        private int _repeat = 1;
        public int Repeat
        {
            get { return _repeat; }
            set { _repeat = value; }
        }

        private class ParameterGenerationTimedOutCommand : TestCommand
        {
            private readonly StackTrace _timedOutTrace;

            private ParameterGenerationTimedOutCommand(IMethodInfo method, string displayName, int timeout, StackTrace timedOutTrace) : base(method, displayName, timeout)
            {
                _timedOutTrace = timedOutTrace;
            }

            public override MethodResult Execute(object testClass)
            {
                var inner = new TimeoutException("Parameter generation timed out");
                throw ExceptionUtility.GetExceptionWithStackTrace(inner, _timedOutTrace);
            }

            public static IEnumerable<ITestCommand> GetSingleCommand(IMethodInfo method, StackTrace timedOutTrace)
            {
                return new TestCommand[]
                           {
                               new ParameterGenerationTimedOutCommand(method, "Parameter generation timed out", -1, timedOutTrace)
                           };
            }
        }

        /// <summary>
        /// Here to avoid unverifiable code
        /// </summary>
        private IEnumerable<ITestCommand> BaseEnumerateTestCommands(IMethodInfo method)
        {
            return base.EnumerateTestCommands(method);
        }
    }
}