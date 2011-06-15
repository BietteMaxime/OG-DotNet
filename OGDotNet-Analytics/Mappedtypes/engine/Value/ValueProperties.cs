//-----------------------------------------------------------------------
// <copyright file="ValueProperties.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using Fudge.Types;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.engine.value
{
    public abstract class ValueProperties : IEquatable<ValueProperties>
    {
        //TODO the rest of this interface
        public abstract ISet<string> Properties { get; }
        public abstract bool IsEmpty { get; }
        public IEnumerable<string> this[string propertyName] { get { return GetValues(propertyName); } }
        public abstract bool IsSatisfiedBy(ValueProperties properties);
        public abstract ISet<string> GetValues(string propertyName);

        public static ValueProperties Create()
        {
            return EmptyValueProperties.Instance;
        }

        public static ValueProperties Create(Dictionary<string, ISet<string>> propertyValues, HashSet<string> optionalProperties)
        {
            return new FiniteValueProperties(propertyValues, optionalProperties);
        }

        private class EmptyValueProperties : ValueProperties
        {
            public static readonly EmptyValueProperties Instance = new EmptyValueProperties();

            private EmptyValueProperties() { }

            public override ISet<string> Properties
            {
                get { return new HashSet<string>(); }
            }

            public override bool IsEmpty
            {
                get { return true; }
            }

            public override bool IsSatisfiedBy(ValueProperties properties)
            {
                return true;
            }

            public override ISet<string> GetValues(string propertyName)
            {
                return null;
            }

            public override bool Equals(ValueProperties other)
            {
                return ReferenceEquals(Instance, other);
            }

            public static new EmptyValueProperties FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
            {
                throw new ArgumentException("This is just here to keep the surrogate selector happy");
            }
        }

        private class FiniteValueProperties : ValueProperties
        {
            public readonly Dictionary<string, ISet<string>> PropertyValues;
            private readonly ISet<string> _optional;


            internal FiniteValueProperties(Dictionary<string, ISet<string>> propertyValues, ISet<string> optional)
            {
                ArgumentChecker.NotEmpty(propertyValues, "propertyValues");
                PropertyValues = propertyValues;
                _optional = optional;
            }

            public override ISet<string> Properties
            {
                get { return new HashSet<string>(PropertyValues.Keys); }
            }

            public override bool IsEmpty
            {
                get { return !PropertyValues.Any(); }
            }

            public override bool IsSatisfiedBy(ValueProperties properties)
            {
                if (InfiniteValueProperties.Instance.IsSatisfiedBy(properties))
                {
                    return true;
                }
                if (properties is EmptyValueProperties)
                {
                    return false;
                }
                if (properties is NearlyInfiniteValueProperties)
                {
                    var niProps = (NearlyInfiniteValueProperties ) properties;
                    return  !niProps.Without.Any(p => PropertyValues.ContainsKey(p) && (_optional == null || ! _optional.Contains(p)));
                }
                if (properties is FiniteValueProperties)
                {
                    var finProps = (FiniteValueProperties) properties;
                    foreach (var propertyValue in PropertyValues)
                    {
                        if (_optional != null && _optional.Contains(propertyValue.Key))
                        {
                            continue;
                        }
                        ISet<string> other;
                        if (!finProps.PropertyValues.TryGetValue(propertyValue.Key, out other))
                        {
                            return false;
                        }
                        if (!other.Any() || !propertyValue.Value.Any())
                        {
                            //wildcards
                            continue;
                        }
                        if (! propertyValue.Value.IsSubsetOf(other))
                        {
                            return false;
                        }
                    }
                    return true;
                }
                throw new ArgumentException();
            }

            public override ISet<string> GetValues(string propertyName)
            {
                ISet<string> ret;
                PropertyValues.TryGetValue(propertyName, out ret);
                return ret;
            }

            public override int GetHashCode()
            {
                int hashCode = PropertyValues.Count;
                hashCode = hashCode * 397 ^ (_optional == null ? 0 : _optional.Count);
                return hashCode;
            }

            public override bool Equals(ValueProperties other)
            {
                var finiteOther = other as FiniteValueProperties;
                if (finiteOther == null)
                {
                    return false;
                }

                if (_optional == null ^ finiteOther._optional == null)
                {
                    return false;
                }
                if (_optional != null && ! _optional.SetEquals(finiteOther._optional))
                {
                    return false;
                }
                return PropertyValues.Count == finiteOther.PropertyValues.Count
                       && PropertyValues.All(delegate(KeyValuePair<string, ISet<string>> entry)
                       {
                           var value = entry.Value;
                           ISet<string> otherValue;
                           if (!finiteOther.PropertyValues.TryGetValue(entry.Key, out otherValue))
                           {
                               return false;
                           }
                           return value.SetEquals(otherValue);
                       });
            }

            public override string ToString()
            {
                return string.Format("[ValueProperties: {0}]",
                                     string.Join(" ", PropertyValues.Select(k => string.Format("{0}->{1}", k.Key, string.Join(",", k.Value)))));
            }

            public static new FiniteValueProperties FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
            {
                throw new ArgumentException("This is just here to keep the surrogate selector happy");
            }
        }

        private class InfiniteValueProperties : ValueProperties
        {
            public static readonly InfiniteValueProperties Instance = new InfiniteValueProperties();

            private InfiniteValueProperties() { }

            public override ISet<string> Properties
            {
                get { return new HashSet<string>(); }
            }

            public override bool IsEmpty
            {
                get { return false; }
            }

            public override bool IsSatisfiedBy(ValueProperties properties)
            {
                return properties == Instance;
            }

            public override ISet<string> GetValues(string propertyName)
            {
                return new HashSet<string>();
            }

            public static new InfiniteValueProperties FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
            {
                throw new ArgumentException("This is just here to keep the surrogate selector happy");
            }

            public override bool Equals(ValueProperties other)
            {
                return ReferenceEquals(other, Instance);
            }
        }

        private class NearlyInfiniteValueProperties : ValueProperties
        {
            internal readonly ISet<string> Without;

            public NearlyInfiniteValueProperties(ISet<string> without)
            {
                Without = without;
            }

            public override ISet<string> Properties
            {
                get { return new HashSet<string>(); }
            }

            public override bool IsEmpty
            {
                get { return false; }
            }

            public override bool IsSatisfiedBy(ValueProperties properties)
            {
                return InfiniteValueProperties.Instance.IsSatisfiedBy(properties) ||
                       (properties is NearlyInfiniteValueProperties &&
                       ((NearlyInfiniteValueProperties) properties).Without.IsSubsetOf(Without));
            }

            public override ISet<string> GetValues(string propertyName)
            {
                return Without.Contains(propertyName) ? null : new HashSet<string>();
            }

            public override bool Equals(ValueProperties other)
            {
                var nearlyInfiniteValueProperties = other as NearlyInfiniteValueProperties;
                if (nearlyInfiniteValueProperties == null)
                {
                    return false;
                }
                return nearlyInfiniteValueProperties.Without.SetEquals(nearlyInfiniteValueProperties.Without);
            }

            public static new NearlyInfiniteValueProperties FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
            {
                throw new ArgumentException("This is just here to keep the surrogate selector happy");
            }
        }

        public static ValueProperties FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var withoutMessage = ffc.GetMessage("without");
            if (withoutMessage != null)
            {
                var without = SmallSet<string>.Create(withoutMessage.GetAllFields().Select(f => f.Value).Cast<string>());
                return without.Any() ? (ValueProperties)new NearlyInfiniteValueProperties(without) : InfiniteValueProperties.Instance;
            }

            var withMessage = ffc.GetMessage("with");

            if (withMessage == null)
            {
                return EmptyValueProperties.Instance;
            }

            IList<IFudgeField> fields = withMessage.GetAllFields();

            var properties = new Dictionary<string, ISet<string>>(fields.Count);
            HashSet<string> optional = null;

            foreach (var field in fields)
            {
                var name = string.Intern(field.Name); // Should be a small static set

                if (field.Value is string)
                {
                    string value = (string)field.Value;

                    properties.Add(name, SmallSet<string>.Create(value));
                }
                else if (field.Type == IndicatorFieldType.Instance)
                {
                    properties.Add(name, new HashSet<string>());
                }
                else
                {
                    var propMessage = (IFudgeFieldContainer)field.Value;
                    
                    IList<IFudgeField> fudgeFields = propMessage.GetAllFields();
                    if (fudgeFields.Count == 1 && fudgeFields[0].Type == IndicatorFieldType.Instance)
                    {
                        optional = optional ?? new HashSet<string>();
                        optional.Add(name);
                    }
                    else
                    {
                        var hashSet = new HashSet<string>();
                        foreach (var fudgeField in fudgeFields)
                        {
                            hashSet.Add((string) fudgeField.Value);
                        }
                        properties.Add(name, hashSet);
                    }
                }
            }

            return new FiniteValueProperties(properties, optional == null ? null : SmallSet<string>.Create(optional));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            if (this is EmptyValueProperties)
            {
                return;
            }
            var finite = this as FiniteValueProperties;
            if (finite != null)
            {
                ToFudgeMsg(finite, a, s);
            }
            else
            {
                var withoutMessage = new FudgeMsg();

                if (this is NearlyInfiniteValueProperties)
                {
                    foreach (var without in ((NearlyInfiniteValueProperties)this).Without)
                    {
                        withoutMessage.Add((string)null, without);
                    }
                }
                a.Add("without", withoutMessage);
            }
        }

        private static void ToFudgeMsg(FiniteValueProperties finite, IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            var withMessage = new FudgeMsg();

            foreach (var property in finite.PropertyValues)
            {
                if (property.Value == null)
                {
                    var optMessage = new FudgeMsg(s.Context);
                    optMessage.Add((string)null, IndicatorType.Instance);

                    withMessage.Add(property.Key, optMessage);
                }
                else if (!property.Value.Any())
                {
                    withMessage.Add(property.Key, IndicatorType.Instance);
                }
                else if (property.Value.Count == 1)
                {
                    withMessage.Add(property.Key, property.Value.Single());
                }
                else
                {
                    var manyMesssage = new FudgeMsg(s.Context);
                    foreach (var val in property.Value)
                    {
                        manyMesssage.Add(property.Key, val);
                    }
                    withMessage.Add(property.Key, manyMesssage);
                }
            }

            a.Add("with", withMessage);
        }

        public abstract bool Equals(ValueProperties other);
    }
}