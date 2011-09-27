//-----------------------------------------------------------------------
// <copyright file="Proxy.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Linq;
using Fudge;
using Fudge.Encodings;
using Fudge.Serialization;
using OGDotNet.Model;
using OGDotNet.Utils;

namespace OGDotNet.Tests.ExecBuilderTestProxy
{
    public static class Proxy
    {
        public static void CycleInputToOutput(string typeHint)
        {
            var openGammaFudgeContext = new OpenGammaFudgeContext();
            var fudgeSerializer = openGammaFudgeContext.GetSerializer();

            using (var openStandardInput = Console.OpenStandardInput())
            using (var openStandardOutput = Console.OpenStandardOutput())
            {
                var fudgeEncodedStreamWriter = new FudgeEncodedStreamWriter(openGammaFudgeContext, openStandardOutput);
                var fudgeEncodedStreamReader = new FudgeEncodedStreamReader(openGammaFudgeContext, openStandardInput);

                var mappingStrategy = (IFudgeTypeMappingStrategy)openGammaFudgeContext.GetProperty(ContextProperties.TypeMappingStrategyProperty);

                var mappedtype = mappingStrategy.GetType(typeHint) ?? typeof(object);

                var hintType = typeof(TestWrapper<>).MakeGenericType(mappedtype);

                var wrapper = (ITestWrapper) Deserialize(fudgeSerializer, hintType, fudgeEncodedStreamReader);

                object hydratedObject = wrapper.TestObject;
                CheckEqualityIfAppropriate(hydratedObject, mappedtype);

                fudgeSerializer.Serialize(fudgeEncodedStreamWriter, wrapper);

                openStandardOutput.Flush();
            }
        }

        public interface ITestWrapper
        {
            object TestObject { get; }
        }
        public class TestWrapper<T> : ITestWrapper where T : class
        {
            public object TestObject { get { return Test; } }
            T Test { get; set; }

            public static TestWrapper<T> FromFudgeMsg(IFudgeFieldContainer ffc,
                                         IFudgeDeserializer deserializer)
            {
                return new TestWrapper<T>() { Test = deserializer.FromField<T>(ffc.GetByName("test")) };
            }

            public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
            {
                s.WriteInline(a, "test", Test);
            }
        }

        private static void CheckEqualityIfAppropriate(object hydratedObject, Type mappedType)
        {
            try
            {
                Console.Error.WriteLine(@"Trying to check equality for {0}", hydratedObject.GetType());
                GenericUtils.Call(typeof(Proxy), "MatchedEquality", typeof(IEquatable<>), hydratedObject);
            }
            catch (ArgumentException e)
            {
                Console.Error.WriteLine(@"Can't check equality for {0} {1}", hydratedObject.GetType(), e.Message);
                return;
            }
            GenericUtils.Call(typeof(Proxy), "CheckEquality", typeof(IEquatable<>), hydratedObject, mappedType);
        }

        public static void MatchedEquality<T>(IEquatable<T> obj)
        {
            if (!(obj is T))
            {
                throw new Exception("Not self equatable");
            }
            Console.Error.WriteLine(@"Matched equality for {0}", typeof(T));
        }

        public static void CheckEquality<T>(IEquatable<T> obj, Type mappedType)
        {
            if (! obj.Equals((T) obj))
            {
                throw new Exception("Object is not self equal");
            }
            var openGammaFudgeContext = new OpenGammaFudgeContext();
            var fudgeSerializer = openGammaFudgeContext.GetSerializer();
            //TODO type hint
            FudgeMsg msg = fudgeSerializer.SerializeToMsg(obj);
            object rehydrated = fudgeSerializer.Deserialize(msg, mappedType);
            if (! obj.Equals((T) rehydrated))
            {
                throw new Exception("roundtripped object not equals");
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