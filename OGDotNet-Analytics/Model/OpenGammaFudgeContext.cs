using Castle.Core;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Model
{
    [Singleton]
    public class OpenGammaFudgeContext : FudgeContext
    {
        private SerializationTypeMap _typeMap;//This is kept around becuase of its very useful cache

        public  OpenGammaFudgeContext()
        {
            SetProperty(ContextProperties.TypeMappingStrategyProperty, new JavaTypeMappingStrategy("OGDotNet.Mappedtypes", "com.opengamma"));
            SetProperty(ContextProperties.FieldNameConventionProperty, FudgeFieldNameConvention.CamelCase);
            _typeMap = new SerializationTypeMap(this);
        }


        public FudgeSerializer GetSerializer()
        {
            return GetSerializer(this);
        }


        private FudgeSerializer GetSerializer(FudgeContext context)
        {
            if (_typeMap == null)
            {
                _typeMap = new SerializationTypeMap(context);
            }
            return new FudgeSerializer(context, _typeMap);
        }
    }
}