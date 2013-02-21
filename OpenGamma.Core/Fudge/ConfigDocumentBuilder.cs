// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigDocumentBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OpenGamma.Core.Config.Impl;
using OpenGamma.Id;
using OpenGamma.Master.Config;

namespace OpenGamma.Fudge
{
    class ConfigDocumentBuilder : GenericBuilderBase
    {
        public void SerializeImpl<TConfig>(ConfigDocument<TConfig> obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            AbstractDocumentHelper.SerializeVersionCorrection(obj, msg);
            if (obj.UniqueId != null)
            {
                msg.Add("uniqueId", obj.UniqueId.ToString());
            }
            serializer.WriteInline(msg, "config", null, obj.Config);
        }

        public override object DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            DateTimeOffset versionFromInstant;
            DateTimeOffset versionToInstant;
            DateTimeOffset correctionFromInstant;
            DateTimeOffset correctionToInstant;
            AbstractDocumentHelper.DeserializeVersionCorrection(msg, out versionFromInstant, out versionToInstant,
                                                                out correctionFromInstant, out correctionToInstant);

            var uidStr = msg.GetString("uniqueId");
            UniqueId uid = (uidStr != null)
                               ? UniqueId.Parse(uidStr)
                               : deserializer.FromField<UniqueId>(msg.GetByName("uniqueId"));
            object config = deserializer.FromField(msg.GetByName("config"), typeof(ConfigItem<>));

            Type configValueType = config.GetType().GetGenericArguments().First();

            Type configDocType = typeof(ConfigDocument<>).MakeGenericType(configValueType);
            return Activator.CreateInstance(configDocType,
                                            new[]
                                                {
                                                    uid, config, null, versionFromInstant, versionToInstant,
                                                    correctionFromInstant, correctionToInstant
                                                });
        }
    }
}
