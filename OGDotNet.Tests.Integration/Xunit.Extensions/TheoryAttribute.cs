using System.Collections.Generic;
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
            bool succeded = ManualTimeout.TryExecuteWithTimeout(() => BaseEnumerateTestCommands(method), out testCommands);
            if (!succeded)
            {
                return ParameterGenerationTimedOutCommand.GetSingleCommand(method);
            }

            return testCommands.Select(cmd => new CustomizingCommand(cmd));
        }

        private class ParameterGenerationTimedOutCommand : TestCommand
        {
            private ParameterGenerationTimedOutCommand(IMethodInfo method, string displayName, int timeout) : base(method, displayName, timeout)
            {
            }

            public override MethodResult Execute(object testClass)
            {
                throw new TimeoutException("Parameter generation timed out");
            }

            public static IEnumerable<ITestCommand> GetSingleCommand(IMethodInfo method)
            {
                return new TestCommand[]
                           {
                               new ParameterGenerationTimedOutCommand(method, "Parameter generation timed out", -1)
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