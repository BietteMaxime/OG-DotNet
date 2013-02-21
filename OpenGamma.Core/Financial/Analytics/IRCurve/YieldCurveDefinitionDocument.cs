// --------------------------------------------------------------------------------------------------------------------
// <copyright file="YieldCurveDefinitionDocument.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge;
using Fudge.Serialization;
using OpenGamma.Fudge;
using OpenGamma.Id;
using OpenGamma.Master;

namespace OpenGamma.Financial.Analytics.IRCurve
{
    public class YieldCurveDefinitionDocument : AbstractDocument
    {
        public YieldCurveDefinitionDocument()
        {
        }

        private YieldCurveDefinitionDocument(DateTimeOffset versionFromInstant, DateTimeOffset versionToInstant, DateTimeOffset correctionFromInstant, DateTimeOffset correctionToInstant) : base(versionFromInstant, versionToInstant, correctionFromInstant, correctionToInstant)
        {
        }

        public YieldCurveDefinition YieldCurveDefinition { get; set; }

        public override UniqueId UniqueId { get; set; }

        public static YieldCurveDefinitionDocument FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            DateTimeOffset versionFromInstant;
            DateTimeOffset versionToInstant;
            DateTimeOffset correctionFromInstant;
            DateTimeOffset correctionToInstant;
            AbstractDocumentHelper.DeserializeVersionCorrection(ffc, out versionFromInstant, out versionToInstant, out correctionFromInstant, out correctionToInstant);

            var uid = (ffc.GetString("uniqueId") != null) ? UniqueId.Parse(ffc.GetString("uniqueId")) : deserializer.FromField<UniqueId>(ffc.GetByName("uniqueId"));
            var definition  = deserializer.FromField<YieldCurveDefinition>(ffc.GetByName("yieldCurveDefinition"));

            return new YieldCurveDefinitionDocument(versionFromInstant, versionToInstant, correctionFromInstant, correctionToInstant) { YieldCurveDefinition = definition, UniqueId = uid};
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer msg, IFudgeSerializer s)
        {
            AbstractDocumentHelper.SerializeVersionCorrection(this, msg);
            if (UniqueId != null)
            {
                msg.Add("uniqueId", UniqueId.ToString());
            }
            msg.Add("yieldCurveDefinition", YieldCurveDefinition);
        }
    }
}