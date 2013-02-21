// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValueProperties.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

using Fudge;
using Fudge.Serialization;
using Fudge.Types;

using OpenGamma.Fudge;
using OpenGamma.Util;

namespace OpenGamma.Engine.Value
{
    [FudgeSurrogate(typeof(ValuePropertiesBuilder))]
    public abstract class ValueProperties : IEquatable<ValueProperties>
    {
        public static readonly ValueProperties All = InfiniteValueProperties.Instance;

        // TODO the rest of this interface
        public abstract ISet<string> Properties { get; }

        public abstract bool IsEmpty { get; }

        public IEnumerable<string> this[string propertyName]
        {
            get
            {
                return GetValues(propertyName);
            }
        }

        public abstract bool IsSatisfiedBy(ValueProperties properties);

        public abstract ISet<string> GetValues(string propertyName);

        public abstract ValueProperties WithoutAny(string propertyName);

        // TODO ValuePropertiesBuilder
        public static ValueProperties Create()
        {
            return EmptyValueProperties.Instance;
        }

        public static ValueProperties Create(
            Dictionary<string, ISet<string>> propertyValues, HashSet<string> optionalProperties)
        {
            return new FiniteValueProperties(propertyValues, optionalProperties);
        }

        public static ValueProperties WithoutAny(params string[] properties)
        {
            return new NearlyInfiniteValueProperties(new HashSet<string>(properties));
        }

        internal class EmptyValueProperties : ValueProperties
        {
            public static readonly EmptyValueProperties Instance = new EmptyValueProperties();

            private EmptyValueProperties()
            {
            }

            public override ISet<string> Properties
            {
                get
                {
                    return new HashSet<string>();
                }
            }

            public override bool IsEmpty
            {
                get
                {
                    return true;
                }
            }

            public override bool IsSatisfiedBy(ValueProperties properties)
            {
                return true;
            }

            public override ISet<string> GetValues(string propertyName)
            {
                return null;
            }

            public override ValueProperties WithoutAny(string propertyName)
            {
                return this;
            }

            public override bool Equals(ValueProperties other)
            {
                return ReferenceEquals(Instance, other);
            }

            public override ValueProperties With(string propertyName, string propertyValue)
            {
                return
                    new FiniteValueProperties(
                        new Dictionary<string, ISet<string>>
                            {
                                { propertyName, SmallSet<string>.Create(propertyValue) }
                            }, 
                        null);
            }
        }

        internal class FiniteValueProperties : ValueProperties
        {
            public readonly Dictionary<string, ISet<string>> PropertyValues;

            private readonly ISet<string> _optional;

            internal FiniteValueProperties(Dictionary<string, ISet<string>> propertyValues, ISet<string> optional)
            {
                if (propertyValues.Count == 0)
                {
                    // This happens so much it's done with Count check
                    throw new ArgumentException("propertyValues");
                }

                PropertyValues = propertyValues;
                _optional = optional;
            }

            public override ISet<string> Properties
            {
                get
                {
                    return new HashSet<string>(PropertyValues.Keys);
                }
            }

            public override bool IsEmpty
            {
                get
                {
                    return !PropertyValues.Any();
                }
            }

            public override bool IsSatisfiedBy(ValueProperties properties)
            {
                // Common case first
                var finProps = properties as FiniteValueProperties;
                if (finProps != null)
                {
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
                            // wildcards
                            continue;
                        }

                        if (!propertyValue.Value.IsSubsetOf(other))
                        {
                            return false;
                        }
                    }

                    return true;
                }

                if (InfiniteValueProperties.Instance.IsSatisfiedBy(properties))
                {
                    return true;
                }

                if (properties is EmptyValueProperties)
                {
                    return false;
                }

                var niProps = properties as NearlyInfiniteValueProperties;
                if (niProps != null)
                {
                    return
                        !niProps.Without.Any(
                            p => PropertyValues.ContainsKey(p) && (_optional == null || !_optional.Contains(p)));
                }

                throw new ArgumentException();
            }

            public override ISet<string> GetValues(string propertyName)
            {
                ISet<string> ret;
                PropertyValues.TryGetValue(propertyName, out ret);
                return ret;
            }

            public override ValueProperties WithoutAny(string propertyName)
            {
                ISet<string> optional = _optional;
                Dictionary<string, ISet<string>> propertyValues = PropertyValues;
                if (optional != null && optional.Contains(propertyName))
                {
                    if (optional.Count == 1)
                    {
                        optional = null;
                    }
                    else
                    {
                        optional = SmallSet<string>.Create(optional.Where(v => v != propertyName));
                    }
                }

                if (propertyValues.ContainsKey(propertyName))
                {
                    propertyValues = propertyValues.Where(k => k.Key != propertyName)
                                                   .ToDictionary(k => k.Key, k => k.Value);
                }

                return new FiniteValueProperties(propertyValues, optional);
            }

            public override int GetHashCode()
            {
                int hashCode = PropertyValues.Count;
                hashCode = hashCode * 397 ^ (_optional == null ? 0 : _optional.Count);
                return hashCode;
            }

            public override bool Equals(object obj)
            {
                var vp = obj as ValueProperties;
                return vp != null && Equals(vp);
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

                if (_optional != null && !_optional.SetEquals(finiteOther._optional))
                {
                    return false;
                }

                if (PropertyValues.Count != finiteOther.PropertyValues.Count)
                {
                    return false;
                }

                foreach (var propertyValue in PropertyValues)
                {
                    var value = propertyValue.Value;
                    ISet<string> otherValue;
                    if (!finiteOther.PropertyValues.TryGetValue(propertyValue.Key, out otherValue))
                    {
                        return false;
                    }

                    if (!value.SetEquals(otherValue))
                    {
                        return false;
                    }
                }

                return true;
            }

            public override string ToString()
            {
                return string.Format(
                    "[ValueProperties: {0}]", 
                    string.Join(
                        " ", PropertyValues.Select(k => string.Format("{0}->{1}", k.Key, string.Join(",", k.Value)))));
            }

            public void Serialize(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
            {
                var context = s.Context;
                var withMessage = new FudgeMsg(context);

                var v = new string[1];

                foreach (var property in PropertyValues)
                {
                    if (property.Value == null)
                    {
                        var optMessage = new FudgeMsg(context) { { (string)null, IndicatorType.Instance } };

                        withMessage.Add(property.Key, optMessage);
                    }
                    else if (0 == property.Value.Count)
                    {
                        withMessage.Add(property.Key, IndicatorType.Instance);
                    }
                    else if (property.Value.Count == 1)
                    {
                        property.Value.CopyTo(v, 0);
                        withMessage.Add(property.Key, v[0]);
                    }
                    else
                    {
                        var manyMesssage = new FudgeMsg(context);
                        foreach (var val in property.Value)
                        {
                            manyMesssage.Add(property.Key, val);
                        }

                        withMessage.Add(property.Key, manyMesssage);
                    }
                }

                if (_optional != null)
                {
                    foreach (var optional in _optional)
                    {
                        withMessage.Add(optional, new FudgeMsg(context, new Field("optional", IndicatorType.Instance)));
                    }
                }

                a.Add("with", withMessage);
            }

            public override ValueProperties With(string propertyName, string propertyValue)
            {
                return
                    new FiniteValueProperties(
                        new Dictionary<string, ISet<string>>(PropertyValues)
                            {
                                {
                                    propertyName, 
                                    SmallSet<string>.Create(
                                        propertyValue)
                                }
                            }, 
                        _optional);
            }
        }

        internal class InfiniteValueProperties : ValueProperties
        {
            public static readonly InfiniteValueProperties Instance = new InfiniteValueProperties();

            private InfiniteValueProperties()
            {
            }

            public override ISet<string> Properties
            {
                get
                {
                    return new HashSet<string>();
                }
            }

            public override bool IsEmpty
            {
                get
                {
                    return false;
                }
            }

            public override bool IsSatisfiedBy(ValueProperties properties)
            {
                return properties == Instance;
            }

            public override ISet<string> GetValues(string propertyName)
            {
                return new HashSet<string>();
            }

            public override ValueProperties WithoutAny(string propertyName)
            {
                return new NearlyInfiniteValueProperties(SmallSet<string>.Create(propertyName));
            }

            public override bool Equals(ValueProperties other)
            {
                return ReferenceEquals(other, Instance);
            }

            public override ValueProperties With(string propertyName, string propertyValue)
            {
                throw new NotImplementedException();
            }
        }

        internal class NearlyInfiniteValueProperties : ValueProperties
        {
            internal readonly ISet<string> Without;

            public NearlyInfiniteValueProperties(ISet<string> without)
            {
                ArgumentChecker.NotEmpty(without, "without");
                Without = without;
            }

            public override ISet<string> Properties
            {
                get
                {
                    return new HashSet<string>();
                }
            }

            public override bool IsEmpty
            {
                get
                {
                    return false;
                }
            }

            public override bool IsSatisfiedBy(ValueProperties properties)
            {
                return InfiniteValueProperties.Instance.IsSatisfiedBy(properties)
                       || (properties is NearlyInfiniteValueProperties
                           && ((NearlyInfiniteValueProperties)properties).Without.IsSubsetOf(Without));
            }

            public override ISet<string> GetValues(string propertyName)
            {
                return Without.Contains(propertyName) ? null : new HashSet<string>();
            }

            public override ValueProperties WithoutAny(string propertyName)
            {
                return new NearlyInfiniteValueProperties(new HashSet<string>(Without) { propertyName });
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

            public override ValueProperties With(string propertyName, string propertyValue)
            {
                throw new NotImplementedException();
            }
        }

        public abstract bool Equals(ValueProperties other);

        public abstract ValueProperties With(string propertyName, string propertyValue);
    }
}