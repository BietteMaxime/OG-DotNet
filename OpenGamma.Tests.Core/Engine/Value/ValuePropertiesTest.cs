// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValuePropertiesTest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using OpenGamma.Fudge;
using OpenGamma.Model;

using Xunit;

namespace OpenGamma.Engine.Value
{
    public class ValuePropertiesTest
    {
        [Fact]
        public void TestEmptyProperties()
        {
            AssertEncodeDecodeCycle(ValueProperties.Create());
        }

        [Fact]
        public void TestAllProperties()
        {
            AssertEncodeDecodeCycle(ValueProperties.All);
        }

        [Fact]
        public void TestNearlyAllProperties()
        {// PLAT-1126
            AssertEncodeDecodeCycle(ValueProperties.WithoutAny(new[] {"SomeProp" }));
        }

        [Fact]
        public void TestValues()
        {
            AssertEncodeDecodeCycle(
                ValueProperties.Create(new Dictionary<string, ISet<string>>
                    {
                        {"Any", new HashSet<string>()}, {"One", new HashSet<string> {"a"}}, {"Two", new HashSet<string> {"b", "c"}}
                    }, new HashSet<string> { "Three" }));
        }

        [Fact]
        public void TestOptionalValues()
        {
            AssertEncodeDecodeCycle(
                ValueProperties.Create(new Dictionary<string, ISet<string>>
                                     {
                                        {"OptAny", new HashSet<string>()}, {"OptSome", new HashSet<string> {"a"}}
                                     }, new HashSet<string> { "OptAny", "OptSome" }));
        }

        private static void AssertEncodeDecodeCycle<T>(T obj)
        {
            var openGammaFudgeContext = new OpenGammaFudgeContext();
            var msg = openGammaFudgeContext.GetSerializer().SerializeToMsg(obj);
            var roundTripped = openGammaFudgeContext.GetSerializer().Deserialize<T>(msg);
            Assert.Equal(obj, roundTripped);
        }
    }
}
