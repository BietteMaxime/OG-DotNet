//-----------------------------------------------------------------------
// <copyright file="YieldCurveDefinitionDocument.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Master;

namespace OGDotNet.Mappedtypes.financial.analytics.ircurve
{
    public class YieldCurveDefinitionDocument : AbstractDocument
    {
        public YieldCurveDefinitionDocument()
        {
        }

        private YieldCurveDefinitionDocument(DateTimeOffset versionFromInstant, DateTimeOffset versionToInstant, DateTimeOffset correctionFromInstant, DateTimeOffset correctionToInstant) : base(versionFromInstant, versionToInstant, correctionFromInstant, correctionToInstant)
        {
        }

        public YieldCurveDefinition Definition { get; set; }

        public override UniqueIdentifier UniqueId { get; set; }

        public static YieldCurveDefinitionDocument FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            DateTimeOffset versionToInstant;
            DateTimeOffset correctionFromInstant;
            DateTimeOffset correctionToInstant;
            DateTimeOffset versionFromInstant = GetDocumentValues(ffc, out versionToInstant, out correctionFromInstant, out correctionToInstant);

            var uid = (ffc.GetString("uniqueId") != null) ? UniqueIdentifier.Parse(ffc.GetString("uniqueId")) : deserializer.FromField<UniqueIdentifier>(ffc.GetByName("uniqueId"));
            var definition  = deserializer.FromField<YieldCurveDefinition>(ffc.GetByName("definition"));

            return new YieldCurveDefinitionDocument(versionFromInstant, versionToInstant, correctionFromInstant, correctionToInstant) { Definition = definition, UniqueId = uid};
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            WriteDocumentFields(a);

            if (UniqueId != null)
            {
                a.Add("uniqueId", UniqueId.ToString());
            }
            a.Add("definition", Definition);
        }
    }
}