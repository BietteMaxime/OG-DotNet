﻿//-----------------------------------------------------------------------
// <copyright file="CecilTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using OGDotNet.Mappedtypes.Id;
using Xunit.Sdk;

namespace OGDotNet.Tests.Integration.Xunit.Extensions
{
    public class CecilTests
    {
        private static AssemblyDefinition GetAssembly()
        {
            var assembly = typeof(UniqueId).Assembly;
            return AssemblyFactory.GetAssembly(assembly.Location);
        }

        public static IEnumerable<TypeDefinition> Types
        {
            get
            {
                foreach (ModuleDefinition module in GetAssembly().Modules)
                {
                    foreach (TypeDefinition type in module.Types)
                    {
                        yield return type;
                    }
                }
            }
        }

        public static IEnumerable<Tuple<MethodDefinition, Instruction>> Instructions
        {
            get
            {
                foreach (TypeDefinition type in Types)
                {
                    if (type.IsInterface)
                    {
                        continue;
                    }
                    foreach (MethodDefinition method in type.Methods)
                    {
                        if (!method.HasBody)
                        {
                            continue;
                        }
                        var methodBody = method.Body;
                        foreach (Instruction instruction in methodBody.Instructions)
                        {
                            yield return Tuple.Create(method, instruction);
                        }
                    }
                }
            }
        }

        public static IEnumerable<Tuple<MethodDefinition, Instruction, MethodReference>> Calls
        {
            get
            {
                foreach (var tup in Instructions)
                {
                    var instruction = tup.Item2;
                    var method = tup.Item1;

                    if (instruction.OpCode.FlowControl != FlowControl.Call)
                        continue;
                    var methodReference = (MethodReference)instruction.Operand;
                    yield return Tuple.Create(method, instruction, methodReference);
                }
            }
        }

        public abstract class CecilTestBase : global::Xunit.FactAttribute
        {
            protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
            {
                try
                {
                    GetAssembly();
                }
                catch (ArgumentException ae)
                {
                    return new ITestCommand[] { new SkipCommand(method, method.Name, string.Format("Cannot run cecil, perhaps the assembly is instrumented: {0}", ae)) };
                }
                return EnumerateTestCommandsImpl(method);
            }

            protected abstract IEnumerable<ITestCommand> EnumerateTestCommandsImpl(IMethodInfo method);
        }

        public class MethodCallsTest : CecilTestBase
        {
            protected override IEnumerable<ITestCommand> EnumerateTestCommandsImpl(IMethodInfo method)
            {
                yield return new CallTestCommand(method);
            }

            private class CallTestCommand : TestCommand
            {
                private readonly IMethodInfo _method;

                public CallTestCommand(IMethodInfo method) : base(method, method.Name, 0)
                {
                    _method = method;
                }

                public override MethodResult Execute(object testClass)
                {
                    var results = Calls.Select(c => Tuple.Create(Invoke(testClass, c), c)).Where(e => e.Item1 != null).ToList();
                    if (results.Any())
                    {
                        var shortMessage = string.Format("{0} fails", results.Count);
                        var messages = results.Select(GetMessage);
                        string message = string.Format("{0} Details:{1}{2}", shortMessage, Environment.NewLine, string.Join(Environment.NewLine, messages));
                        return new FailedResult(_method, new AggregateException(message, results.Select(r => r.Item1)),  DisplayName);
                    }
                    return new PassedResult(testMethod, DisplayName);
                }

                private static string GetMessage(Tuple<Exception, Tuple<MethodDefinition, Instruction, MethodReference>> r)
                {
                    return string.Format("{0}: Method {1} calls {2} incorrectly", r.Item1.Message, r.Item2.Item1, r.Item2.Item3);
                }

                private Exception Invoke(object testClass, Tuple<MethodDefinition, Instruction, MethodReference> call)
                {
                    try
                    {
                        _method.Invoke(testClass, call.Item1, call.Item2, call.Item3);
                        return null;
                    }
                    catch (Exception e)
                    {
                        return e;
                    }
                }
            }
        }
        public class TypesTest : CecilTestBase
        {
            protected override IEnumerable<ITestCommand> EnumerateTestCommandsImpl(IMethodInfo method)
            {
                yield return new TypesTestCommand(method);
            }

            private class TypesTestCommand : TestCommand
            {
                private readonly IMethodInfo _method;

                public TypesTestCommand(IMethodInfo method)
                    : base(method, method.Name, 0)
                {
                    _method = method;
                }

                public override MethodResult Execute(object testClass)
                {
                    var results = Types.Select(c => Tuple.Create(Invoke(testClass, c), c)).Where(e => e.Item1 != null).ToList();
                    if (results.Any())
                    {
                        var shortMessage = string.Format("{0} fails", results.Count);
                        var messages = results.Select(GetMessage);
                        string message = string.Format("{0} Details:{1}{2}", shortMessage, Environment.NewLine, string.Join(Environment.NewLine, messages));
                        return new FailedResult(_method, new AggregateException(message, results.Select(r => r.Item1)), DisplayName);
                    }

                    return new PassedResult(testMethod, DisplayName);
                }

                private static string GetMessage(Tuple<Exception, TypeDefinition> r)
                {
                    return string.Format("{0}: Type {1}", r.Item1.Message, r.Item2);
                }

                private Exception Invoke(object testClass, TypeDefinition type)
                {
                    try
                    {
                        _method.Invoke(testClass, type);
                        return null;
                    }
                    catch (Exception e)
                    {
                        return e;
                    }
                }
            }
        }  
    }
}
