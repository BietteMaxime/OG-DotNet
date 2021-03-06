﻿//-----------------------------------------------------------------------
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
using OGDotNet.Mappedtypes.Engine.Value;
using OGDotNet.Utils;

using EmptyValueProperties = OGDotNet.Mappedtypes.Engine.Value.ValueProperties.EmptyValueProperties;
using FiniteValueProperties = OGDotNet.Mappedtypes.Engine.Value.ValueProperties.FiniteValueProperties;
using InfiniteValueProperties = OGDotNet.Mappedtypes.Engine.Value.ValueProperties.InfiniteValueProperties;
using NearlyInfiniteValueProperties = OGDotNet.Mappedtypes.Engine.Value.ValueProperties.NearlyInfiniteValueProperties;

namespace OGDotNet.Builders
{
    class ValuePropertiesBuilder : BuilderBase<ValueProperties>
    {
        public ValuePropertiesBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override void SerializeImpl(ValueProperties obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            var finite = obj as FiniteValueProperties;
            if (finite != null)
            {
                finite.Serialize(msg, serializer);
            }
            else
            {
                if (obj is EmptyValueProperties)
                {
                    return;
                }
                var withoutMessage = new FudgeMsg(serializer.Context);

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
                if (withoutMessage.Any())
                {
                    var without = withoutMessage.Select(f => f.Value).Cast<string>();
                    var withoutSet = SmallSet<string>.Create(without);
                    return new NearlyInfiniteValueProperties(withoutSet);
                }
                else
                {
                    return InfiniteValueProperties.Instance;
                }
            }

            var withMessage = ffc.GetMessage("with");

            if (withMessage == null)
            {
                return EmptyValueProperties.Instance;
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
                    //withAny
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
                            string value = (string) fudgeField.Value;
                            hashSet.Add(value);
                        }
                    }
                    if (hashSet.Any())
                    {
                        properties.Add(name, hashSet);
                    }
                }
            }

            return new FiniteValueProperties(properties, optional == null ? null : SmallSet<string>.Create(optional));
        }
    }
}
