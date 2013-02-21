// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValuePropertiesBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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

using OpenGamma.Engine.Value;
using OpenGamma.Util;

namespace OpenGamma.Fudge
{
    class ValuePropertiesBuilder : BuilderBase<ValueProperties>
    {
        public ValuePropertiesBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override void SerializeImpl(ValueProperties obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            var finite = obj as ValueProperties.FiniteValueProperties;
            if (finite != null)
            {
                finite.Serialize(msg, serializer);
            }
            else
            {
                if (obj is ValueProperties.EmptyValueProperties)
                {
                    return;
                }

                var withoutMessage = new FudgeMsg(serializer.Context);

                if (obj is ValueProperties.NearlyInfiniteValueProperties)
                {
                    foreach (var without in ((ValueProperties.NearlyInfiniteValueProperties)obj).Without)
                    {
                        withoutMessage.Add((string)null, without);
                    }
                }

                msg.Add("without", withoutMessage);
            }
        }

        protected override ValueProperties DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var withoutMessage = msg.GetMessage("without");
            if (withoutMessage != null)
            {
                if (withoutMessage.Any())
                {
                    var without = withoutMessage.Select(f => f.Value).Cast<string>();
                    var withoutSet = SmallSet<string>.Create(without);
                    return new ValueProperties.NearlyInfiniteValueProperties(withoutSet);
                }
                return ValueProperties.InfiniteValueProperties.Instance;
            }

            var withMessage = msg.GetMessage("with");

            if (withMessage == null)
            {
                return ValueProperties.EmptyValueProperties.Instance;
            }

            var properties = new Dictionary<string, ISet<string>>(withMessage.GetNumFields());
            HashSet<string> optional = null;

            foreach (var field in withMessage)
            {
                var name = string.Intern(field.Name); // Should be a small static set

                var fieldType = field.Type;
                if (Equals(fieldType, StringFieldType.Instance))
                {
                    var value = (string)field.Value;
                    properties.Add(name, SmallSet<string>.Create(value));
                }
                else if (Equals(fieldType, IndicatorFieldType.Instance))
                {
                    // withAny
                    properties.Add(name, new HashSet<string>());
                }
                else if (Equals(fieldType, FudgeMsgFieldType.Instance))
                {
                    var propMessage = (IFudgeFieldContainer) field.Value;
                    var hashSet = new HashSet<string>();
                    foreach (var fudgeField in propMessage)
                    {
                        if (fudgeField.Value == IndicatorType.Instance)
                        {
                            if (fudgeField.Name != "optional")
                            {
                                throw new ArgumentException();
                            }

                            optional = optional ?? new HashSet<string>();
                            optional.Add(name);
                        }
                        else
                        {
                            var value = (string) fudgeField.Value;
                            hashSet.Add(value);
                        }
                    }

                    if (hashSet.Any())
                    {
                        properties.Add(name, hashSet);
                    }
                }
            }

            return new ValueProperties.FiniteValueProperties(properties, optional == null ? null : SmallSet<string>.Create(optional));
        }
    }
}
