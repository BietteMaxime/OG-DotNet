using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using Fudge.Types;

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

                string value;
                if (field.Value is string || field.Value == null)
                {
                    value = (string) field.Value;
                }
                else if (IsIndicatorType(field.Value))
                {
                    value = null;
                }
                else
                {
                    throw new ArgumentException(string.Format("Unexpected value {0}",field.Value));
                }

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

        private static bool IsIndicatorType(object value)
        {
            var msg = value as IFudgeFieldContainer;
            if (msg == null)
                return false;

            if (msg.GetAllFields().Count != 1)
                return false;
            var fudgeField = msg.GetByName(null);
            if (fudgeField == null)
                return false;
            return fudgeField.Value is IndicatorType;
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