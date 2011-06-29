//-----------------------------------------------------------------------
// <copyright file="TestsAreIntegrationTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OGDotNet.Tests.Integration.OGDotNet.Model;
using OGDotNet.Tests.Integration.OGDotNet.Model.Context;
using OGDotNet.Tests.Integration.OGDotNet.Resources;
using Xunit;
using FactAttribute = OGDotNet.Tests.Integration.Xunit.Extensions.FactAttribute;

namespace OGDotNet.Tests.Integration
{
    public class TestsAreIntegrationTests
    {
        static readonly HashSet<Type> Exclusions = new HashSet<Type>()
                                                      {
                                                          typeof(RemoteEngineContextTests),
                                                          typeof(RemoteEngineContextFactoryTests),
                                                          typeof(TestsAreIntegrationTests),

                                                          typeof(OpenGammaFudgeContextTests), //Here because it is slow
                                                     };
        [Fact]
        public void CheckAll()
        {
            var testTypes = typeof(TestsAreIntegrationTests).Assembly.GetTypes().Where(IsTestClass).ToList();
            foreach (var testType in testTypes.Except(Exclusions))
            {
                Assert.True(typeof(TestWithContextBase).IsAssignableFrom(testType), string.Format("Test class {0} doesn't use context", testType.Name));
            }
        }

        private static bool IsTestClass(Type t)
        {
            return t.GetMethods().Any(IsTestMethod);
        }

        private static bool IsTestMethod(MethodInfo arg)
        {
            return arg.GetCustomAttributes(typeof(FactAttribute), true).Any();
        }
    }
}
