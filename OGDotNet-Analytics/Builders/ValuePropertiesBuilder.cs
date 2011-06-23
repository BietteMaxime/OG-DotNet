//-----------------------------------------------------------------------
// <copyright file="ValuePropertiesBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Utils;

using EmptyValueProperties = OGDotNet.Mappedtypes.engine.value.ValueProperties.EmptyValueProperties;
using FiniteValueProperties = OGDotNet.Mappedtypes.engine.value.ValueProperties.FiniteValueProperties;
using InfiniteValueProperties = OGDotNet.Mappedtypes.engine.value.ValueProperties.InfiniteValueProperties;
using NearlyInfiniteValueProperties = OGDotNet.Mappedtypes.engine.value.ValueProperties.NearlyInfiniteValueProperties;

namespace OGDotNet.Builders
{
    class ValuePropertiesBuilder : BuilderBase<ValueProperties>
    {
        public ValuePropertiesBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override void SerializeImpl(ValueProperties obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            if (obj is EmptyValueProperties)
            {
                return;
            }
            var finite = obj as FiniteValueProperties;
            if (finite != null)
            {
                finite.Serialize(msg, serializer);
            }
            else
            {
                var withoutMessage = new FudgeMsg();

                if (obj is NearlyInfiniteValueProperties)
                {
                    foreach (var without in ((NearlyInfiniteValueProperties)obj).Without)
                    {
                        withoutMessage.Add((string)null, without);
                    }
                }
                msg.Add("without", withoutMessage);
            }
        }

        public override ValueProperties DeserializeImpl(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
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
                            hashSet.Add((string)fudgeField.Value);
                        }
                        properties.Add(name, hashSet);
                    }
                }
            }

            return new FiniteValueProperties(properties, optional == null ? null : SmallSet<string>.Create(optional));
        }
    }
}
