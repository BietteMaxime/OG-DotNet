using System;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Model
{
    public static class FudgeConfig
    {
        [ThreadStatic]
        private static FudgeContext _fudgeContext;

        [ThreadStatic]
        private static SerializationTypeMap _typeMap;//This is kept around becuase of its very useful cache

        public static FudgeSerializer GetFudgeSerializer()
        {
            FudgeContext fudgeContext = GetFudgeContext();
            if (_typeMap == null)
            {
                _typeMap = new SerializationTypeMap(_fudgeContext);
            }
            return new FudgeSerializer(fudgeContext, _typeMap);
        }

        public static FudgeContext GetFudgeContext()
        {
          
            if (_fudgeContext == null)
            {
                _fudgeContext = new FudgeContext();
                _fudgeContext.SetProperty(ContextProperties.TypeMappingStrategyProperty, new JavaTypeMappingStrategy("OGDotNet.Mappedtypes", "com.opengamma"));
                _fudgeContext.SetProperty(ContextProperties.FieldNameConventionProperty, FudgeFieldNameConvention.CamelCase);
            }
            return _fudgeContext;
        }
    }
}