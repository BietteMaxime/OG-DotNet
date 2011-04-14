//-----------------------------------------------------------------------
// <copyright file="OpenGammaFudgeContextTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Fudge.Serialization;
using OGDotNet.Model;
using Xunit;
using A = OGDotNet.Mappedtypes.OpenGammaFudgeContextTestsTypes.A;
using B = OGDotNet.Mappedtypes.OpenGammaFudgeContextTestsTypes.B;
using C = OGDotNet.Mappedtypes.OpenGammaFudgeContextTestsTypes.C;
using FactAttribute = OGDotNet.Tests.Integration.Xunit.Extensions.FactAttribute;
namespace OGDotNet.Tests.Integration.OGDotNet.Model
{
    public class OpenGammaFudgeContextTests
    {
        [Fact]
        public void SerializerKindOfWorks()
        {
            var context = new OpenGammaFudgeContext();
            Thrash(context);
        }

        [Fact]
        public void SerializerIsThreadSafe()
        {
            for (int i = 0; i < 1000; i++)
            {
                var context = new OpenGammaFudgeContext();
                var parallelLoopResult = Parallel.For(1, 4 * Environment.ProcessorCount, _ => Thrash(context));
            }
        }

        /// <summary>
        /// This tries to check that the cacheing which breaks <see cref="SerializerIsThreadSafe"/> actually works
        /// </summary>
        [Fact(Timeout = 12000)]
        public void SerializerIsFast()
        {
            var context = new OpenGammaFudgeContext();
            
            for (int i = 0; i < 100000; i++)
            {
                Thrash(context);
            }
        }

        private static FudgeSerializer Thrash(OpenGammaFudgeContext context)
        {
            var fudgeSerializer = context.GetSerializer();
            var graph = new A { B = new B { C = new C { A = new A { B = new B() } } } };
            var msg = fudgeSerializer.SerializeToMsg(graph);
            A graphRet = fudgeSerializer.Deserialize<A>(msg);
            Assert.NotNull(graphRet.B.C.A.B);
            Assert.Null(graphRet.B.C.A.B.C);
            return fudgeSerializer;
        }
    }
}