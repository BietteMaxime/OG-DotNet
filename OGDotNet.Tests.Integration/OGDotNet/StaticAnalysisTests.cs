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
using OGDotNet.Mappedtypes.Core.MarketDataSnapshot.Impl;
using OGDotNet.Mappedtypes.Engine;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using OGDotNet.Tests.OGDotNet.Mappedtypes;
using OGDotNet.Tests.Xunit.Extensions;
using OGDotNet.Utils;
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
                        throw new Exception(string.Format("{0} has property {1}, use UniqueId instead (And {2})", mappedType.Name, propertyInfo.Name, typeof(IUniqueIdentifiable).Name));
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

        [CecilTests.TypesTest]
        public void FactoryMethodsNamedDotNetStyle(TypeDefinition type)
        {
            List<MethodDefinition> methodInfos = type.Methods.Cast<MethodDefinition>().Where(m => m.IsStatic && !m.IsSpecialName && type.Equals(m.ReturnType.ReturnType.Resolve())).ToList();
            foreach (var methodInfo in methodInfos)
            {
                if (methodInfo.Name == "Of" || methodInfo.Name == "From")
                {
                    throw new Exception(string.Format("{0} is a Create method named Of in the java convention", methodInfo.Name));
                }
                if (methodInfo.Name == "FromFudgeMsg")
                {
                    continue;
                }
                if (methodInfo.Name == "Create")
                {
                    continue;
                }
                if (methodInfo.Name.StartsWith("Without"))
                {
                    continue;
                }
            }
        }

        private static TypeDefinition GetValueAssertions()
        {
            var asm = typeof(ValueAssertions).Assembly;
            foreach (ModuleDefinition module in AssemblyFactory.GetAssembly(asm.Location).Modules)
            {
                return module.Types[typeof(ValueAssertions).FullName];
            }
            throw new Exception();
        }

        [Xunit.Extensions.Fact]
        public void ValueAssertionsArentUseless()
        {
            var knownUselessAssertions = new HashSet<string>
                                             {
                                                 typeof(string).FullName
                                             };
            foreach (var typeName in EmptyTypesWarner.KnownEmptyTypes.Select(t => t.FullName))
            {
                knownUselessAssertions.Add(typeName);
            }

            var valueAssertions = GetValueAssertions();
            foreach (var method in valueAssertions.Methods.Cast<MethodDefinition>().Where(m => m.Name == "AssertSensibleValue" && m.Parameters.Count == 1))
            {
                Assert.NotNull(method);
                var instructions = method.Body.Instructions.Cast<Instruction>().Where(i => i.OpCode != OpCodes.Nop).ToList();
                if (instructions.Count > 3)
                {
                    continue;
                }
                string type = method.Parameters[0].ParameterType.FullName;
                if (instructions.Count < 3)
                {
                    Assert.False(true, "Tiny body for assertion on " + type);
                }
                if (instructions[0].OpCode == OpCodes.Ldarg_0 && instructions[1].OpCode == OpCodes.Call && instructions[2].OpCode == OpCodes.Ret)
                {
                    var operand = instructions[1].Operand;
                    Assert.Equal("NotNull", ((MethodReference) operand).Name);
                    Assert.True(knownUselessAssertions.Contains(type), "Useless assertion on " + type);
                }
                else
                {
                    Assert.False(true, "Don't understand assertion on " + type);
                }
            }
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

        [CecilTests.MethodCallsTest]
        public void NoBannedMethods(MethodDefinition method, Instruction instruction, MethodReference methodReference)
        {
            if (methodReference.DeclaringType.FullName == typeof(IFudgeFieldContainer).FullName)
            {
                if (methodReference.Name == "GetAllFields")
                {
                    throw new Exception("GetAllFields is ~20% slower than raw enumeration (and enumeration can be made faster in the future)");
                }
            }
            if (methodReference.DeclaringType.FullName == typeof(FudgeMsg).FullName)
            {
                if (methodReference.Name == ".ctor")
                {
                    if (!methodReference.Parameters.Cast<ParameterDefinition>().Any(d => d.ParameterType.FullName == typeof(FudgeContext).FullName))
                    {
                        throw new Exception("Constructing  a FudgeMsg without a FudgeContext is slow");
                    }
                }
            }
        }

        [CecilTests.MethodCallsTest]
        public void ArgumentCheckerCalledCorrectly(MethodDefinition method, Instruction instruction, MethodReference methodReference)
        {
            if (methodReference.DeclaringType.FullName == typeof(ArgumentChecker).FullName)
            {
                var parameters = methodReference.Parameters.Cast<ParameterDefinition>().ToList();
                var argNameParam = parameters.Where(p => p.Name == "argName").Single();
                Assert.Equal(argNameParam, parameters.Last());
                var ldStr = instruction.Previous;
                if (ldStr.OpCode != OpCodes.Ldstr)
                {
                    //Some fancy pants dynamic call
                    Assert.Equal(typeof(ArgumentChecker).FullName, method.DeclaringType.FullName);
                    return;
                }
                var ldArg = ldStr.Previous;

                string argName = (string)ldStr.Operand;
                if (argName.Contains("."))
                {
                    //TODO multiple .s
                    //Some fancy pants fishing call
                    var callVirt = ldArg;
                    ldArg = callVirt.Previous;
                    Assert.Equal(OpCodes.Callvirt, callVirt.OpCode);
                    var operand = (MethodReference)callVirt.Operand;

                    var property = argName.Substring(argName.IndexOf(".") + 1);
                    Assert.Equal(operand.Name, "get_" + property);
                    
                    argName = argName.Substring(0, argName.IndexOf("."));
                }

                if (!method.Parameters.Cast<ParameterDefinition>().Any(p => p.Name == argName))
                {
                    throw new Exception(string.Format("Wrong arg name {0}", argName));
                }

                if (methodReference.Name != "Not")
                {//Only things which should be called directly on an arg
                    ParameterDefinition parameterDefinition = GetParameterDefinition(method, ldArg);
                    if (parameterDefinition == null)
                    {
                        return;
                    }
                    if (parameterDefinition.Name != argName)
                    {
                        throw new Exception(string.Format(
                            "Wrong arg name {0} != {1}", argName, parameterDefinition.Name));
                    }
                }
            }
        }

        private static ParameterDefinition GetParameterDefinition(MethodDefinition method, Instruction ldArg)
        {
            if (ldArg.OpCode == OpCodes.Ldarg_S)
            {
                return (ParameterDefinition)ldArg.Operand;
            }
            int index = GetLdArgIndex(ldArg);
            if (index < 0)
            {
                return null;
            }
            if (method.HasThis)
            {
                index -= 1;
            }
            return method.Parameters[index];
        }

        private static int GetLdArgIndex(Instruction ldArg)
        {
            if (ldArg.OpCode == OpCodes.Ldarg_0)
            {
                return 0;
            }
            else if (ldArg.OpCode == OpCodes.Ldarg_1)
            {
                return 1;
            }
            else if (ldArg.OpCode == OpCodes.Ldarg_2)
            {
                return 2;
            }
            else if (ldArg.OpCode == OpCodes.Ldarg_3)
            {
                return 3;
            }
            else
            {
                return -1;
            }
        }

        [CecilTests.TypesTest]
        public void BuildersAllUseful(TypeDefinition type)
        {
            if (type.BaseType != null && type.BaseType.Name.StartsWith("BuilderBase"))
            {
                var methods = type.Methods.Cast<MethodDefinition>();
                Assert.NotEmpty(methods);

                var serialize = methods.SingleOrDefault(m => m.Name == "SerializeImpl");
                var deserialize = methods.SingleOrDefault(m => m.Name == "DeserializeImpl");
                Assert.NotNull(deserialize);
                if (!(IsUseful(deserialize) || IsUseful(serialize)))
                {
                    throw new Exception("Neither deserialize nor serialize is useful");
                }
            }
        }

        [CecilTests.TypesTest]
        public void BuildersAreSymmetric(TypeDefinition type)
        {
            if (type.Name == "ValuePropertiesBuilder")
            {
                //This builder is too clever for its own good
                return;
            }
            if (type.BaseType != null && type.BaseType.Name.StartsWith("BuilderBase"))
            {
                var methods = type.Methods.Cast<MethodDefinition>();
                Assert.NotEmpty(methods);

                var serialize = methods.SingleOrDefault(m => m.Name == "SerializeImpl");
                var deserialize = methods.SingleOrDefault(m => m.Name == "DeserializeImpl");
                Assert.NotNull(deserialize);
                if (IsUseful(deserialize) && IsUseful(serialize))
                {
                    var deserializeStrings = new HashSet<string>(GetStaticStrings(deserialize));
                    var serializeStrings = new HashSet<string>(GetStaticStrings(serialize));
                    if (! deserializeStrings.SetEquals(serializeStrings))
                    {
                        deserializeStrings.SymmetricExceptWith(serializeStrings);
                        throw new Exception(string.Format("Mismatched strings: {0}", string.Join(",", deserializeStrings)));
                    }
                }
            }
        }

        private static IEnumerable<string> GetStaticStrings(MethodDefinition method)
        {
            return method.Body.Instructions.Cast<Instruction>().Where(i => i.OpCode == OpCodes.Ldstr).Select(i => i.Operand).Cast<string>();
        }

        private static bool IsUseful(MethodDefinition serialize)
        {
            if (serialize == null)
            {
                return false;
            }
            var instructionCollection = serialize.Body.Instructions;
            if (instructionCollection.Count > 3)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [CecilTests.TypesTest]
        public void SnapshotObjectsConsistentUpdateable(TypeDefinition type)
        {
            if (type.Namespace.StartsWith(typeof(ManageableMarketDataSnapshot).Namespace))
            {
                if (type.IsInterface)
                {
                    return;
                }
                var publicMethods = type.Methods.Cast<MethodDefinition>().Where(m => m.IsPublic);
                Assert.NotEmpty(publicMethods);

                var haveOverrides = publicMethods.SingleOrDefault(m => m.Name == "HaveOverrides");
                var removeAllOverrides = publicMethods.SingleOrDefault(m => m.Name == "RemoveAllOverrides");
                var prepareUpdate = publicMethods.SingleOrDefault(m => m.Name == "PrepareUpdateFrom");

                if ((haveOverrides == null) != (removeAllOverrides == null) || (haveOverrides == null) != (prepareUpdate == null))
                {
                    throw new Exception("Missing method from group");
                }
                if (haveOverrides != null)
                {
                    var haRefs = new HashSet<FieldReference>(GetReferences(haveOverrides));
                    var raoRefs = new HashSet<FieldReference>(GetReferences(removeAllOverrides));
                    var puRefs = new HashSet<FieldReference>(GetReferences(prepareUpdate));
                    Assert.NotEmpty(haRefs);
                    
                    if (! haRefs.SetEquals(raoRefs))
                    {
                        throw new Exception("HaveOverrides and RemoveAllOverrides use different fields");
                    }
                    if (!haRefs.SetEquals(puRefs))
                    {
                        throw new Exception("HaveOverrides and PrepareUpdateFrom use different fields");
                    }

                    foreach (FieldDefinition field in type.Fields)
                    {
                        if (field.IsStatic)
                            continue;
                        if (! field.FieldType.Name.Contains("Dictionary"))
                            continue;
                        var fieldReferences = haRefs.Where(f => f.Resolve() == field).ToList();
                        if (fieldReferences.Count != 1)
                        {
                            throw new Exception(string.Format("Field {0} is not used", field.Name));
                        }
                    }
                }
            }
        }

        private static IEnumerable<FieldReference> GetReferences(MethodDefinition haveOverrides)
        {
            foreach (Instruction inst in haveOverrides.Body.Instructions)
            {
                if (inst.OpCode == OpCodes.Ldfld)
                {
                    yield return (FieldReference) inst.Operand;
                }
            }
        }
    }
}
