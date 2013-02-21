// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PortfolioDocumentBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Id;
using OpenGamma.Master;
using OpenGamma.Master.Portfolio;

namespace OpenGamma.Fudge
{
    public class PortfolioDocumentBuilder : BuilderBase<PortfolioDocument>
    {
        public PortfolioDocumentBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override PortfolioDocument DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            DateTimeOffset versionFromInstant;
            DateTimeOffset versionToInstant;
            DateTimeOffset correctionFromInstant;
            DateTimeOffset correctionToInstant;
            AbstractDocumentHelper.DeserializeVersionCorrection(msg, out versionFromInstant, out versionToInstant, out correctionFromInstant, out correctionToInstant);

            var uid = (msg.GetString("uniqueId") != null) ? UniqueId.Parse(msg.GetString("uniqueId")) : deserializer.FromField<UniqueId>(msg.GetByName("uniqueId"));
            var portfolio = deserializer.FromField<ManageablePortfolio>(msg.GetByName("portfolio"));

            return new PortfolioDocument(versionFromInstant, versionToInstant, correctionFromInstant, correctionToInstant, uid, portfolio);
        }

        protected override void SerializeImpl(PortfolioDocument obj, IAppendingFudgeFieldContainer a, IFudgeSerializer serializer)
        {
            AbstractDocumentHelper.SerializeVersionCorrection(obj, a);
            if (obj.UniqueId != null)
            {
                a.Add("uniqueId", obj.UniqueId.ToString());
            }
            a.Add("portfolio", obj.Portfolio);
        }
    }
}
