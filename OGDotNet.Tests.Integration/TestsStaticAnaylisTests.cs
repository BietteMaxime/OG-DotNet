//-----------------------------------------------------------------------
// <copyright file="TestsStaticAnaylisTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OGDotNet.Tests.Integration.OGDotNet;
using OGDotNet.Tests.Integration.OGDotNet.Builders;
using OGDotNet.Tests.Integration.OGDotNet.Model;
using OGDotNet.Tests.Integration.OGDotNet.Model.Context;
using OGDotNet.Tests.Integration.OGDotNet.Resources;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using OGDotNet.Tests.Xunit.Extensions;
using Xunit;

namespace OGDotNet.Tests.Integration
{
    public class TestsStaticAnaylisTests
    {
        public static IEnumerable<Type> TestTypes
        {
            get
            {
                var testTypes = typeof(TestsStaticAnaylisTests).Assembly.GetTypes().Where(IsTestClass).ToList();
                return testTypes;
            }
        }

        static readonly HashSet<Type> NonIntegrationTests = new HashSet<Type>
                                           {
                                                          typeof(RemoteEngineContextTests),
                                                          typeof(RemoteEngineContextFactoryTests),
                                                          typeof(TestsStaticAnaylisTests),
                                                          typeof(StaticAnalysisTests),
                                                          typeof(CecilTests),
                                                          typeof(MemoizingTypeMappingStrategyTest),

                                                          typeof(OpenGammaFudgeContextTests), //Here because it is slow
                                                     };
        [Theory]
        [TypedPropertyData("TestTypes")]
        public void TestsAreIntegrationTests(Type testType)
        {
            if (NonIntegrationTests.Contains(testType))
                return;
            Assert.True(typeof(TestWithContextBase).IsAssignableFrom(testType), string.Format("Test class {0} doesn't use context", testType.Name));
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

        private static IEnumerable<global::Xunit.FactAttribute> GetRawAttributes(MethodInfo m)
        {
            return m.GetCustomAttributes(typeof(global::Xunit.FactAttribute), true).Where(a => a.GetType() == typeof(global::Xunit.FactAttribute) || a.GetType() == typeof(global::Xunit.Extensions.TheoryAttribute)).Cast<global::Xunit.FactAttribute>().ToList();
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
