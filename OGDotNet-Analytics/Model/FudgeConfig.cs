using System;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet_Analytics.Model
{
    public static class FudgeConfig
    {
        [ThreadStatic]
        private static FudgeContext _fudgeContext;

        [ThreadStatic]
        private static SerializationTypeMap _typeMap;

        public static FudgeSerializer GetFudgeSerializer()
        {
            FudgeContext fudgeContext = GetFudgeContext();
            return new FudgeSerializer(fudgeContext, _typeMap);
        }

        public static FudgeContext GetFudgeContext()
        {
          
            if (_fudgeContext == null)
            {
                _fudgeContext = new FudgeContext();
                _fudgeContext.SetProperty(ContextProperties.TypeMappingStrategyProperty, new JavaTypeMappingStrategy("OGDotNet_Analytics.Mappedtypes", "com.opengamma"));
                _fudgeContext.SetProperty(ContextProperties.FieldNameConventionProperty, FudgeFieldNameConvention.CamelCase);
                _typeMap = new SerializationTypeMap(_fudgeContext);
            }
            return _fudgeContext;
        }
    }
}