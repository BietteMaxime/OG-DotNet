//-----------------------------------------------------------------------
// <copyright file="StaticAnalysisTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Mono.Cecil;
using Mono.Cecil.Cil;
using OGDotNet.Mappedtypes.Core.MarketDataSnapshot;
using OGDotNet.Mappedtypes.Engine;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using OGDotNet.Tests.Xunit.Extensions;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet
{
    public class StaticAnalysisTests
    {
        public static IEnumerable<Type> Types
        {
            get
            {
                var mappedTypes = typeof(UniqueId).Assembly.GetTypes().ToList();
                Assert.NotEmpty(mappedTypes);
                return mappedTypes.OrderBy(t => t.FullName);
            }
        }

        [Theory]
        [TypedPropertyData("Types")]
        public void ConsistentUniqueIdentifierPropertyName(Type mappedType)
        {
            foreach (var propertyInfo in mappedType.GetProperties().Where(p => p.PropertyType == typeof(UniqueId)))
            {
                switch (propertyInfo.Name)
                {
                    case "Identifier":
                    case "Id":
                    case "UniqueIdentifier":
                        throw new Exception(string.Format("{0} has property {1}, use UniqeId instead (And {2})", mappedType.Name, propertyInfo.Name, typeof(IUniqueIdentifiable).Name));
                }
            }
        }

        [Theory]
        [TypedPropertyData("Types")]
        public void ExplicitImplementationofIUniqueIdentifiable(Type mappedType)
        {
            if (
                mappedType.GetProperties().Where(p => p.PropertyType == typeof(UniqueId) && p.Name == "UniqueId").Any()
                && !typeof(IUniqueIdentifiable).IsAssignableFrom(mappedType))
            {
                if (mappedType != typeof(ComputationTarget)
                    &&
                    mappedType != typeof(MarketDataValueSpecification))
                {
                    throw new Exception(string.Format("{0} duck types as {1} but doesn't implement it",
                                                      mappedType.Name, typeof(IUniqueIdentifiable).Name));
                }
            }
        }

        [Theory]
        [TypedPropertyData("Types")]
        public void InterfacesNamedAsInterfaces(Type mappedType)
        {
            if (mappedType.IsInterface)
            {
                var name = mappedType.Name;
                if (name[0] != 'I' || !char.IsUpper(name[1]))
                {
                    Assert.Equal(name, "ISomething");
                }
            }
        }

        [Theory]
        [TypedPropertyData("Types")]
        public void NamespacesStartWithUppercase(Type type)
        {
            for (int i = 0; i < type.Namespace.Length; i++)
            {
                if (type.Namespace[i] == '.')
                {
                    if (!char.IsUpper(type.Namespace[i + 1]))
                    {
                        string nextNameSpace = type.Namespace.Substring(i + 1).Split('.')[0];
                        if (Types.Any(t => string.Equals(t.Name, nextNameSpace, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            Assert.True(nextNameSpace.All(c => char.IsLower(c)));
                            //Irritating conflict due to C#s casing convention
                            continue;
                        }
                        else
                        {
                            throw new ArgumentException(string.Format("Namespace starts with lower case letter {0}",
                                                                      type.Namespace));
                        }
                    }
                }
            }
        }

        [Xunit.Extensions.Fact]
        public void NoBannedMethods()
        {
            List<string> fails = new List<string>();
            var assembly = typeof(UniqueId).Assembly;
            var assemblyDefinition = AssemblyFactory.GetAssembly(assembly.Location);
            foreach (ModuleDefinition module in assemblyDefinition.Modules)
            {
                foreach (TypeDefinition type in module.Types)
                {
                    if (type.IsInterface)
                    {
                        continue;
                    }
                    foreach (MethodDefinition method in type.Methods)
                    {
                        if (! method.HasBody)
                        {
                            continue;
                        }
                        var methodBody = method.Body;
                        foreach (Instruction instruction in methodBody.Instructions)
                        {
                            if (instruction.OpCode.FlowControl != FlowControl.Call)
                                continue;
                            var methodReference = (MethodReference)instruction.Operand;
                            string reason;
                            if (IsBannedCall(methodReference, out reason))
                            {
                                fails.Add(string.Format("{0}: Method {1} calls {2} which is banned", reason, method, methodReference));
                            }
                        }
                    }
                }   
            }
            if (fails.Any())
            {
                var message = Environment.NewLine + string.Join(Environment.NewLine, fails);
                Assert.True(false, message);
                throw new Exception(message);
            }
        }

        private static bool IsBannedCall(MethodReference methodReference, out string reason)
        {
            if (methodReference.DeclaringType.FullName == typeof(FudgeMsg).FullName)
            {
                if (methodReference.Name == ".ctor")
                {
                    if (!methodReference.Parameters.Cast<ParameterDefinition>().Any(d => d.ParameterType.FullName == typeof(FudgeContext).FullName))
                    {
                        reason = "Constructing  a FudgeMsg without a FudgeContext is slow";
                        return true;
                    }
                }
            }
            reason = null;
            return false;
        }

        [Xunit.Extensions.Fact]
        public void NamespaceCasingConsistent()
        {
            var nameSpaces = new HashSet<string>();

            foreach (var type in Types)
            {
                nameSpaces.Add(type.Namespace);
                for (int i = 0; i < type.Namespace.Length; i++)
                {
                    if (type.Namespace[i] == '.')
                    {
                        nameSpaces.Add(type.Namespace.Substring(0, i));
                    }
                }

                foreach (var g in nameSpaces.ToLookup(s => s.ToUpperInvariant()))
                {
                    if (g.Count() > 1)
                    {
                        throw new Exception(string.Format("Found many casings {0}: {1}", g.Key, string.Join(",", g)));
                    }
                }
            }
        }
    }
}
