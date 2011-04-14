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

namespace OGDotNet.Mappedtypes.engine.value
{
    public abstract class ValueProperties
    {
        //TODO the rest of this interface
        public abstract ISet<string> Properties { get; }
        public abstract bool IsEmpty { get; }
        public abstract IEnumerable<string> this[string curve] { get; }

        public static ValueProperties Create()
        {
            return EmptyValueProperties.Instance;
        }

        public static ValueProperties Create(Dictionary<string, HashSet<string>> propertyValues)
        {
            return new FiniteValueProperties(propertyValues);
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

            public static new EmptyValueProperties FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
            {
                throw new ArgumentException("This is just here to keep the surrogate selector happy");
            }
        }

        private class FiniteValueProperties : ValueProperties
        {
            internal readonly Dictionary<string, HashSet<string>> PropertyValues;

            internal FiniteValueProperties(Dictionary<string, HashSet<string>> propertyValues)
            {
                PropertyValues = propertyValues;
            }

            public override ISet<string> Properties
            {
                get { return new HashSet<string>(PropertyValues.Keys);}
            }

            public override bool IsEmpty
            {
                get { return ! PropertyValues.Any(); }
            }

            public override IEnumerable<string> this[string curve]
            {
                get {
                    HashSet<string> ret;
                    PropertyValues.TryGetValue(curve, out ret);
                    return ret;
                }
            }

            public static new FiniteValueProperties FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
            {
                throw new ArgumentException("This is just here to keep the surrogate selector happy");
            }
        }

        private class InfiniteValueProperties : ValueProperties
        {
            public static readonly InfiniteValueProperties Instance = new InfiniteValueProperties();

            private InfiniteValueProperties(){}

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

            public static new NearlyInfiniteValueProperties FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
            {
                throw new ArgumentException("This is just here to keep the surrogate selector happy");
            }
        }




        public static ValueProperties FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var properties = new Dictionary<string, HashSet<string>>();
            var withoutMessage = ffc.GetMessage("without");
            if (withoutMessage!= null)
            {
                var without = new HashSet<string>(withoutMessage.GetAllFields().Select(f => f.Value).Cast<string>());
                return without.Any() ? (ValueProperties) new NearlyInfiniteValueProperties(without) : InfiniteValueProperties.Instance;
            }

            var withMessage = ffc.GetMessage("with");

            if (withMessage == null)
            {
                return EmptyValueProperties.Instance;
            }

            foreach (var field in withMessage)
            {
                var name = field.Name;

                if (field.Value is string)
                {
                    string value = (string) field.Value;

                    properties.Add(name, new HashSet<string> {value});
                }
                else if (field.Type == IndicatorFieldType.Instance)
                {
                    properties.Add(name, new HashSet<string>());
                }
                else
                {
                    var propMessage = (IFudgeFieldContainer) field.Value;
                    if (propMessage.Count() == 1 && propMessage.Single().Type == IndicatorFieldType.Instance)
                    {
                        properties.Add(name,null);
                    }
                    else
                    {
                        var hashSet = new HashSet<string>(propMessage.Select(f => f.Value).Cast<string>());
                        properties.Add(name, hashSet);                        
                    }
                }
            }

            return new FiniteValueProperties(properties);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            if (this is EmptyValueProperties)
            {
                return;
            }
            if (this is FiniteValueProperties)
            {
                var withMessage = new FudgeMsg();

                foreach (var property in ((FiniteValueProperties) this).PropertyValues)
                {
                    if (property.Value == null)
                    {
                        var optMessage = new FudgeMsg(s.Context);
                        optMessage.Add((string) null, IndicatorType.Instance);

                        withMessage.Add(property.Key, optMessage);
                    }
                    else if (! property.Value.Any())
                    {
                        withMessage.Add(property.Key, IndicatorType.Instance);
                    }
                    else if (property.Value.Count==1)
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
            else
            {
                var withoutMessage = new FudgeMsg();
                
                if (this is NearlyInfiniteValueProperties)
                {
                    foreach (var without in ((NearlyInfiniteValueProperties) this).Without)
                    {
                        withoutMessage.Add((string) null, without);
                    }
                }
                a.Add("without", withoutMessage);
            }
        }


        public bool IsSatisfiedBy(ValueProperties properties)
        {
            if (IsEmpty)
                return true;
            throw new NotImplementedException();
        }
    }
}