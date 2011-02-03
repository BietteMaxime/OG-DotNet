using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet_Analytics.Mappedtypes.engine.View
{
    public class ValueProperties
    {
        private readonly Dictionary<string, HashSet<string>> _properties;

        private ValueProperties(Dictionary<string, HashSet<string>> properties)
        {
            _properties = properties;
        }

        public Dictionary<string, HashSet<string>> Properties//TODO kill this
        {
            get { return _properties; }
        }

        public static ValueProperties FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            //TODO this properly
            var properties = new Dictionary<string, HashSet<string>>();
            foreach (var field in ffc.GetAllByName("portfolioRequirement"))
            {
                HashSet<string> propertyValueSet;
                if (! properties.TryGetValue(field.Name, out propertyValueSet))
                {
                    propertyValueSet = new HashSet<string>();
                    properties[field.Name] = propertyValueSet;
                }
                propertyValueSet.Add((string)field.Value);
            }
            return new ValueProperties(properties);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }

    }
}