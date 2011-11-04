//-----------------------------------------------------------------------
// <copyright file="PortfolioDocumentBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
using OGDotNet.Mappedtypes.Master.Portfolio;

namespace OGDotNet.Builders
{
    class PortfolioDocumentBuilder : BuilderBase<PortfolioDocument>
    {
        public PortfolioDocumentBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override PortfolioDocument DeserializeImpl(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            DateTimeOffset versionToInstant;
            DateTimeOffset correctionFromInstant;
            DateTimeOffset correctionToInstant;
            DateTimeOffset versionFromInstant = AbstractDocument.GetDocumentValues(ffc, out versionToInstant, out correctionFromInstant, out correctionToInstant);

            var uid = (ffc.GetString("uniqueId") != null) ? UniqueId.Parse(ffc.GetString("uniqueId")) : deserializer.FromField<UniqueId>(ffc.GetByName("uniqueId"));
            var portfolio = deserializer.FromField<ManageablePortfolio>(ffc.GetByName("portfolio"));

            return new PortfolioDocument(versionFromInstant, versionToInstant, correctionFromInstant, correctionToInstant, uid, portfolio);
        }

        protected override void SerializeImpl(PortfolioDocument obj, IAppendingFudgeFieldContainer a, IFudgeSerializer serializer)
        {
            obj.WriteDocumentFields(a);

            if (obj.UniqueId != null)
            {
                a.Add("uniqueId", obj.UniqueId.ToString());
            }
            a.Add("portfolio", obj.Portfolio);
        }
    }
}
