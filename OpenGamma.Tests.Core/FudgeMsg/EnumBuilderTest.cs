// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumBuilderTest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Fudge;
using OpenGamma.Xunit.Extensions;

using Xunit;
using Xunit.Extensions;

namespace OpenGamma.FudgeMsg
{
    public class EnumBuilderTest
    {
        public enum MyEnum
        {
            A, 
            AComplexName, 
            ComplexName, 
            ANABBREVIATION, 
            Abc6A, 
        }
        [Theory]
        [EnumValuesData]
        public void RoundTrip(MyEnum e)
        {
            var javaName = EnumBuilder<MyEnum>.GetJavaName(e);
            foreach (var c in javaName)
            {
                Assert.True(char.IsUpper(c) || c == '_' || char.IsNumber(c));
            }

            var roundTripped = EnumBuilder<MyEnum>.Parse(javaName);
            Assert.Equal(e, roundTripped);
        }
    }
}
