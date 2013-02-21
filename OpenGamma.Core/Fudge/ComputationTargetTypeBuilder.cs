// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComputationTargetTypeBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fudge;
using Fudge.Serialization;
using OpenGamma.Engine.Target;

namespace OpenGamma.Fudge
{
    internal class ComputationTargetTypeBuilder : BuilderBase<ComputationTargetType>
    {
        internal const string TypeField = "computationTargetType";

        private static readonly IDictionary<string, ComputationTargetType> CommonTargetTypes = new Dictionary<string, ComputationTargetType>();

        static ComputationTargetTypeBuilder()
        {
            foreach (FieldInfo field in typeof(ComputationTargetType).GetFields())
            {
                if (field.IsPublic && field.IsStatic && typeof(ComputationTargetType).IsAssignableFrom(field.FieldType))
                {
                    var common = (ComputationTargetType) field.GetValue(null);
                    CommonTargetTypes[common.ToString()] = common;
                }
            }
        }
        
        public ComputationTargetTypeBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public static void SerializeCore(ComputationTargetType obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            obj.Serialize(TypeField, msg, serializer);
        }

        protected override void SerializeImpl(ComputationTargetType obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            SerializeCore(obj, msg, serializer);
        }

        private static ComputationTargetType Deserialize(ComputationTargetType outer, IFudgeField field, Aggregator aggregator, Aggregator innerAggregator, IFudgeDeserializer deserializer)
        {
            var typeName = field.Value as string;
            if (typeName != null)
            {
                ComputationTargetType common = CommonTargetTypes[typeName];
                if (common != null)
                {
                    return outer == null ? common : outer.Containing(common);
                }
                var typeMappingStrategy = (IFudgeTypeMappingStrategy)deserializer.Context.GetProperty(ContextProperties.TypeMappingStrategyProperty);
                Type type = typeMappingStrategy.GetType(typeName);
                Type classTargetType = typeof(ClassComputationTargetType<>).MakeGenericType(new[] {type});
                var targetType = (ComputationTargetType) Activator.CreateInstance(classTargetType);
                return outer == null ? targetType : aggregator(outer, targetType);
            }
            var msg = field.Value as IFudgeFieldContainer;
            if (msg != null)
            {
                var type = msg.Aggregate((ComputationTargetType) null, (current, field2) => Deserialize(current, field, innerAggregator, aggregator, deserializer));
                if (type != null)
                {
                    return outer == null ? type : aggregator(outer, type);
                }
                return outer;
            }
            return outer;
        }

        private delegate ComputationTargetType Aggregator(ComputationTargetType a, ComputationTargetType b);

        private static ComputationTargetType Containing(ComputationTargetType outer, ComputationTargetType inner)
        {
            return outer == null ? inner : outer.Containing(inner);
        }

        private static ComputationTargetType Or(ComputationTargetType current, ComputationTargetType additional)
        {
            return current == null ? additional : current.Or(additional);
        }

        internal static ComputationTargetType DeserializeCore(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return msg.Where(field => field.Name == TypeField)
                      .Aggregate((ComputationTargetType)null,
                                 (outer, field) => Deserialize(outer, field, Or, Containing, deserializer));
        }

        protected override ComputationTargetType DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return DeserializeCore(msg, deserializer);
        }
    }
}