using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet;

namespace OGDotNet_Analytics
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
                var old = (IFudgeTypeMappingStrategy)_fudgeContext.GetProperty(ContextProperties.TypeMappingStrategyProperty, new JoiningMappingStrategty());
                _fudgeContext.SetProperty(ContextProperties.TypeMappingStrategyProperty, new JoiningMappingStrategty(old, new JavaTypeMappingStrategy("OGDotNet_Analytics.Mappedtypes", "com.opengamma"), new LaxTypeMappingStrategy()));
                _fudgeContext.SetProperty(ContextProperties.FieldNameConventionProperty, FudgeFieldNameConvention.CamelCase);
                _typeMap = new SerializationTypeMap(_fudgeContext);
            }
            return _fudgeContext;
        }
    }
}