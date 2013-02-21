// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenGammaFudgeContextTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;

using Fudge.Serialization;

using Xunit;

namespace OpenGamma.Fudge
{
    public class OpenGammaFudgeContextTests
    {
        [Xunit.Extensions.Fact]
        public void SerializerKindOfWorks()
        {
            var context = new OpenGammaFudgeContext();
            Thrash(context);
        }

        [Xunit.Extensions.Fact]
        public void SerializerIsThreadSafe()
        {
            for (int i = 0; i < 1000; i++)
            {
                var context = new OpenGammaFudgeContext();
                Parallel.For(1, 4 * Environment.ProcessorCount, _ => Thrash(context));
            }
        }

        /// <summary>
        /// This tries to check that the caching which breaks <see cref="SerializerIsThreadSafe"/> actually works
        /// </summary>
        [Xunit.Extensions.Fact(Timeout = 12000, Skip = "This test is machine dependent")]
        public void SerializerIsFast()
        {
            var context = new OpenGammaFudgeContext();
            
            for (int i = 0; i < 200000; i++)
            {
                Thrash(context);
            }
        }

        private static FudgeSerializer Thrash(OpenGammaFudgeContext context)
        {
            var fudgeSerializer = context.GetSerializer();
            var graph = new OpenGammaFudgeContextTestsTypes.A { B = new OpenGammaFudgeContextTestsTypes.B { C = new OpenGammaFudgeContextTestsTypes.C { A = new OpenGammaFudgeContextTestsTypes.A { B = new OpenGammaFudgeContextTestsTypes.B() } } } };
            var msg = fudgeSerializer.SerializeToMsg(graph);
            OpenGammaFudgeContextTestsTypes.A graphRet = fudgeSerializer.Deserialize<OpenGammaFudgeContextTestsTypes.A>(msg);
            Assert.NotNull(graphRet.B.C.A.B);
            Assert.Null(graphRet.B.C.A.B.C);
            return fudgeSerializer;
        }
    }
}