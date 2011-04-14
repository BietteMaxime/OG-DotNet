//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using Fudge.Encodings;
using Fudge.Serialization;
using OGDotNet.Model;

namespace OGDotNet.Tests.ExecBuilderTestProxy
{
    /// <summary>
    /// A proxy which can be used by com.opengamma.engine.fudgemsg.BuilderTestProxyFactory.ExecBuilderTestProxy in order to test the serialization
    /// TODO: make this get run as part of the build
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("Unexpected number of arguments");
            }
            string typeHint = args[0];

            var openGammaFudgeContext = new OpenGammaFudgeContext();
            var fudgeSerializer = openGammaFudgeContext.GetSerializer();

            using (var openStandardInput = Console.OpenStandardInput())
            using (var openStandardOutput = Console.OpenStandardOutput())
            {
                var fudgeEncodedStreamWriter = new FudgeEncodedStreamWriter(openGammaFudgeContext, openStandardOutput);
                var fudgeEncodedStreamReader = new FudgeEncodedStreamReader(openGammaFudgeContext, openStandardInput);

                var mappingStrategy = (IFudgeTypeMappingStrategy)openGammaFudgeContext.GetProperty(ContextProperties.TypeMappingStrategyProperty);

                var mappedtype = mappingStrategy.GetType(typeHint);

                object hydratedObject;

                if (mappedtype != null)
                {
                    hydratedObject = Deserialize(fudgeSerializer, mappedtype, fudgeEncodedStreamReader);
                }
                else
                {
                    hydratedObject = fudgeSerializer.Deserialize(fudgeEncodedStreamReader);
                }

                fudgeSerializer.Serialize(fudgeEncodedStreamWriter, hydratedObject);

                openStandardOutput.Flush();
            }
        }

        private static object Deserialize(FudgeSerializer fudgeSerializer, Type mappedtype, FudgeEncodedStreamReader fudgeEncodedStreamReader)
        {
            var methodInfo = fudgeSerializer.GetType().GetMethods().Where(
                m => m.Name == "Deserialize"
                    && m.GetParameters().Count() == 1 && m.GetParameters().Single().ParameterType.IsAssignableFrom(fudgeEncodedStreamReader.GetType())
                    && m.ContainsGenericParameters
                ).Select(m => m.MakeGenericMethod(new[] { mappedtype })).Single();

            return methodInfo.Invoke(fudgeSerializer, new object[] { fudgeEncodedStreamReader });
        }
    }
}
