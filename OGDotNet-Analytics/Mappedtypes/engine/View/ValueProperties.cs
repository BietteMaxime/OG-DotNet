using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Mappedtypes.engine.View
{
    public class ValueProperties
    {
        private readonly Dictionary<string, HashSet<string>> _properties;

        internal ValueProperties() : this(new Dictionary<string, HashSet<string>>())
        {
            
        }
        internal ValueProperties(Dictionary<string, HashSet<string>> properties)
        {
            _properties = properties;
        }

        public Dictionary<string, HashSet<string>> Properties//TODO kill this
        {
            get { return _properties; }
        }

        public static ValueProperties FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var properties = new Dictionary<string, HashSet<string>>();
            foreach (var field in ffc)
            {
                var name = field.Name;
                var value = (string) field.Value;

                HashSet<string> propertyValueSet;
                if (!properties.TryGetValue(name, out propertyValueSet))
                {
                    propertyValueSet = new HashSet<string>();
                    properties[name] = propertyValueSet;
                }
                propertyValueSet.Add(value);
            }

            return new ValueProperties(properties);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }

        public ValueProperties Filter(Func<KeyValuePair<string,HashSet<string>>, bool> predicate)
        {
            return new ValueProperties(_properties.Where(predicate).ToDictionary(k => k.Key, v => v.Value));
        }
    }
}