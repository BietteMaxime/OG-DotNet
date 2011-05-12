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
        public abstract IEnumerable<string> this[string curve] { get; }
        public abstract bool IsSatisfiedBy(ValueProperties properties);
        public abstract ISet<string> GetValues(string propertyName);

        public static ValueProperties Create()
        {
            return EmptyValueProperties.Instance;
        }

        public static ValueProperties Create(Dictionary<string, HashSet<string>> propertyValues, HashSet<string> optionalProperties)
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

            public override IEnumerable<string> this[string curve]
            {
                get { return Enumerable.Empty<string>(); }
            }

            public override bool IsSatisfiedBy(ValueProperties properties)
            {
                return true;
            }

            public override ISet<string> GetValues(string propertyName)
            {
                return null;
            }

            public static new EmptyValueProperties FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
            {
                throw new ArgumentException("This is just here to keep the surrogate selector happy");
            }
        }

        private class FiniteValueProperties : ValueProperties
        {
            public readonly Dictionary<string, HashSet<string>> PropertyValues;
            private readonly HashSet<string> _optional;
            private readonly Lazy<int> _hashCode;

            internal FiniteValueProperties(Dictionary<string, HashSet<string>> propertyValues, HashSet<string> optional)
            {
                ArgumentChecker.NotEmpty(propertyValues, "propertyValues");
                PropertyValues = propertyValues;
                _optional = optional;
                _hashCode = new Lazy<int>(GetHashCodeImpl);
            }

            public override ISet<string> Properties
            {
                get { return new HashSet<string>(PropertyValues.Keys); }
            }

            public override bool IsEmpty
            {
                get { return !PropertyValues.Any(); }
            }

            public override IEnumerable<string> this[string curve]
            {
                get
                {
                    HashSet<string> ret;
                    PropertyValues.TryGetValue(curve, out ret);
                    return ret;
                }
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
                    return  !niProps.Without.Any(p => PropertyValues.ContainsKey(p) && ! _optional.Contains(p));
                }
                if (properties is FiniteValueProperties)
                {
                    var finProps = (FiniteValueProperties) properties;
                    foreach (var propertyValue in PropertyValues)
                    {
                        if (_optional.Contains(propertyValue.Key))
                        {
                            continue;
                        }
                        HashSet<string> other;
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
                HashSet<string> ret;
                PropertyValues.TryGetValue(propertyName, out ret);
                return ret;
            }

            public override int GetHashCode()
            {
                return _hashCode.Value;
            }

            private int GetHashCodeImpl()
            {
                unchecked
                {
                    return PropertyValues.Keys.OrderBy(s=>s).Aggregate(0, (i,s)=>i*397 ^ s.GetHashCode());
                }
            }

            public override string ToString()
            {
                return string.Format("[ValueProperties: {0}]",
                                     string.Join(" ",PropertyValues.Select(k => string.Format("{0}->{1}", k.Key, string.Join(",", k.Value)))));
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

            public override IEnumerable<string> this[string curve]
            {
                get { return Enumerable.Empty<string>(); }
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

            public override IEnumerable<string> this[string curve]
            {
                get { return Without.Contains(curve) ? null : Enumerable.Empty<string>(); }
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
                var without = new HashSet<string>(withoutMessage.GetAllFields().Select(f => f.Value).Cast<string>());
                return without.Any() ? (ValueProperties)new NearlyInfiniteValueProperties(without) : InfiniteValueProperties.Instance;
            }

            var withMessage = ffc.GetMessage("with");

            if (withMessage == null)
            {
                return EmptyValueProperties.Instance;
            }

            IList<IFudgeField> fields = withMessage.GetAllFields();
            
            var properties = new Dictionary<string, HashSet<string>>(fields.Count);
            var optional = new HashSet<string>();

            foreach (var field in fields)
            {
                var name = field.Name;

                if (field.Value is string)
                {
                    string value = (string)field.Value;

                    properties.Add(name, new HashSet<string> { value });
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

            return new FiniteValueProperties(properties, optional);
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

        public bool Equals(ValueProperties other)
        {
            return other != null && other.IsSatisfiedBy(this) && IsSatisfiedBy(other);
        }


    }
}