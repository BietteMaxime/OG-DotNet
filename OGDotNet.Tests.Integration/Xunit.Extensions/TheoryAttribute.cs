using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Xunit;
using Xunit.Sdk;

namespace OGDotNet.Tests.Integration.Xunit.Extensions
{
    public class TheoryAttribute : global::Xunit.Extensions.TheoryAttribute
    {
        private static readonly ConcurrentDictionary<Assembly, Assembly> CheckedAssemblies = new ConcurrentDictionary<Assembly, Assembly>();

        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            var enumerateTestCommands = base.EnumerateTestCommands(method);
            
            return enumerateTestCommands.Select(cmd => new TracingTest(cmd)).Concat(GetAssemblyCheckingCommands(method.MethodInfo.DeclaringType.Assembly));
        }


        private static IEnumerable<ITestCommand> GetAssemblyCheckingCommands(Assembly assembly)
        {
            if (!CheckedAssemblies.TryAdd(assembly, assembly))
                yield break;
            foreach (var type in assembly.GetTypes())
            {
                foreach (var methodInfo in type.GetMethods())
                {
                    foreach (var customAttribute in methodInfo.GetCustomAttributes(false))
                    {
                        if (customAttribute is global::Xunit.Extensions.TheoryAttribute &&
                            ! (customAttribute is TheoryAttribute))
                        {
                            yield return
                                new LamdaTestCommandFactory(() =>
                                    Assert.False(true,
                                    string.Format("{0}.{1} is using the wrong theory attribute",type.FullName, methodInfo.Name)));
                            
                        }
                    }
                }
            }
        }

        private class LamdaTestCommandFactory : ITestCommand
        {

            private readonly Action _action;

            public LamdaTestCommandFactory(Action action)
            {
                _action = action;
            }


            public MethodResult Execute(object testClass)
            {
                _action();
                return new PassedResult(null, "Lambda"); //TODO 
            }

            public XmlNode ToStartXml()
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml("<dummy/>");
                XmlNode node = XmlUtility.AddElement(document.ChildNodes[0], "start");
                return node;
            }


            public string DisplayName
            {
                get { return string.Format("Lambda test {0}", _action); }
            }

            public bool ShouldCreateInstance
            {
                get { return false; }
            }

            public int Timeout
            {
                get { return int.MaxValue; }
            }
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