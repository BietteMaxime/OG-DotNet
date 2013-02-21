// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigItemBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;
using OpenGamma.Core.Config.Impl;
using OpenGamma.Id;

namespace OpenGamma.Fudge
{
    internal class ConfigItemBuilder : GenericBuilderBase
    {
        public void SerializeImpl<TConfig>(ConfigItem<TConfig> obj, IAppendingFudgeFieldContainer msg,
                                           IFudgeSerializer serializer)
        {
            if (obj.UniqueId != null)
            {
                msg.Add("uniqueId", obj.UniqueId.ToString());
            }
            msg.Add("name", obj.Name);

            var typeMappingStrategy = (IFudgeTypeMappingStrategy)
                serializer.Context.GetProperty(ContextProperties.TypeMappingStrategyProperty);
            string valueTypeName = typeMappingStrategy.GetName(obj.Value.GetType());
            msg.Add("type", valueTypeName);
            msg.Add("value", obj.Value);
        }

        public override object DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            string uniqueIdStr = msg.GetString("uniqueId");
            UniqueId uid = (uniqueIdStr != null)
                               ? UniqueId.Parse(uniqueIdStr)
                               : deserializer.FromField<UniqueId>(msg.GetByName("uniqueId"));
            string name = msg.GetString("name");

            var typeMappingStrategy = (IFudgeTypeMappingStrategy)
                deserializer.Context.GetProperty(ContextProperties.TypeMappingStrategyProperty);
            var valueTypeName = msg.GetString("type");
            Type valueType = typeMappingStrategy.GetType(valueTypeName);
            object value = deserializer.FromField(msg.GetByName("value"), valueType);

            Type configItemType = typeof(ConfigItem<>).MakeGenericType(new[] {valueType});
            return Activator.CreateInstance(configItemType, new[] {uid, value, name});
        }
    }
}
