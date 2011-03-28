using System;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Builders
{
    static class SerializerExtensions
    {
        public static void WriteTypeHeader(this IFudgeSerializer s, IAppendingFudgeFieldContainer a, Type type)
        {
            //Unlike the java library this functionality is not exposed, so we have to duplicate it
            var map = (IFudgeTypeMappingStrategy)(s.Context.GetProperty(ContextProperties.TypeMappingStrategyProperty));
            a.Add(0, map.GetName(type));
        }
    }
}
