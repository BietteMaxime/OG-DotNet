using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace OGDotNet.Tests.Integration.Xunit.Extensions
{
    public class TheoryAttribute : global::Xunit.Extensions.TheoryAttribute
    {
        private static readonly ConcurrentDictionary<Assembly, Assembly> CheckedAssemblies = new ConcurrentDictionary<Assembly, Assembly>();

        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            var enumerateTestCommands = base.EnumerateTestCommands(method);
            
            return enumerateTestCommands.Select(cmd => new TracingTest(cmd));
        }

        private class TracingTest  : DelegatingTestCommand
        {
            public TracingTest(ITestCommand innerCommand) : base(innerCommand)
            {
            }

            public override MethodResult Execute(object testClass)
            {
                Console.Out.WriteLine("Executing {0}", InnerCommand.DisplayName);
                var result = base.InnerCommand.Execute(testClass);
                Console.Out.WriteLine("Executed {0}", InnerCommand.DisplayName);
                return result;
            }
        }
    }
} 