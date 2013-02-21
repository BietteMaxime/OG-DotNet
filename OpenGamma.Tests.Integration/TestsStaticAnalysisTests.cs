// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestsStaticAnalysisTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using OpenGamma.Fudge;
using OpenGamma.Model;
using OpenGamma.Model.Context;
using OpenGamma.Model.Resources;
using OpenGamma.Xunit.Extensions;

using Xunit;

using FactAttribute = Xunit.FactAttribute;

namespace OpenGamma
{
    public class TestsStaticAnalysisTests
    {
        public static IEnumerable<Type> TestTypes
        {
            get
            {
                var testTypes = typeof(TestsStaticAnalysisTests).Assembly.GetTypes().Where(IsTestClass).ToList();
                return testTypes;
            }
        }

        static readonly HashSet<Type> NonIntegrationTests = new HashSet<Type>
                                           {
                                                          typeof(RemoteEngineContextTests), 
                                                          typeof(RemoteEngineContextFactoryTests), 
                                                          typeof(TestsStaticAnalysisTests), 
                                                          typeof(StaticAnalysisTests), 
                                                          typeof(CecilTests), 
                                                          typeof(MemoizingTypeMappingStrategyTest), 

                                                          typeof(OpenGammaFudgeContextTests), // Here because it is slow
                                                     };
        [Theory]
        [TypedPropertyData("TestTypes")]
        public void TestsAreIntegrationTests(Type testType)
        {
            if (NonIntegrationTests.Contains(testType))
                return;
            Assert.True(typeof(RemoteEngineContextTestBase).IsAssignableFrom(testType), string.Format("Test class {0} doesn't use context", testType.Name));
        }

        [Theory]
        [TypedPropertyData("TestTypes")]
        public void TestsUseExtendedAttributes(Type testType)
        {
            if (NonIntegrationTests.Contains(testType))
                return;

            var ts = testType.GetMethods().Select(m => Tuple.Create(m, GetRawAttributes(m))).Where(t => t.Item2.Any()).ToList();
            var rawTests = ts.Select(t => t.Item1);
            if (rawTests.Any())
            {
                throw new Exception(string.Format(" Methods on {0} use raw attributes: {1}", testType.FullName, string.Join(", ", rawTests.Select(r => r.ToString()))));
            }
        }

        private static IEnumerable<FactAttribute> GetRawAttributes(MethodInfo m)
        {
            return m.GetCustomAttributes(typeof(global::Xunit.FactAttribute), true).Where(a => a.GetType() == typeof(global::Xunit.FactAttribute) || a.GetType() == typeof(global::Xunit.Extensions.TheoryAttribute)).Cast<FactAttribute>().ToList();
        }

        private static bool IsTestClass(Type t)
        {
            return t.GetMethods().Any(IsTestMethod);
        }

        private static bool IsTestMethod(MethodInfo arg)
        {
            return arg.GetCustomAttributes(typeof(global::Xunit.FactAttribute), true).Any();
        }
    }
}
