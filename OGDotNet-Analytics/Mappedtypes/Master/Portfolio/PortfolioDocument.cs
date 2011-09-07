//-----------------------------------------------------------------------
// <copyright file="PortfolioDocument.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Master.Portfolio
{
    public class PortfolioDocument : AbstractDocument
    {
        private readonly ManageablePortfolio _portfolio;
        private UniqueId _uniqueId;

        public PortfolioDocument(DateTimeOffset versionFromInstant, DateTimeOffset versionToInstant, DateTimeOffset correctionFromInstant, DateTimeOffset correctionToInstant, UniqueId uniqueId, ManageablePortfolio portfolio)
            : base(versionFromInstant, versionToInstant, correctionFromInstant, correctionToInstant)
        {
            _uniqueId = uniqueId;
            _portfolio = portfolio;
        }

        public override UniqueId UniqueId
        {
            get { return _uniqueId; }
            set { _uniqueId = value; }
        }

        public ManageablePortfolio Portfolio
        {
            get { return _portfolio; }
        }

        public static PortfolioDocument FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            DateTimeOffset versionToInstant;
            DateTimeOffset correctionFromInstant;
            DateTimeOffset correctionToInstant;
            DateTimeOffset versionFromInstant = GetDocumentValues(ffc, out versionToInstant, out correctionFromInstant, out correctionToInstant);

            var uid = (ffc.GetString("uniqueId") != null) ? UniqueId.Parse(ffc.GetString("uniqueId")) : deserializer.FromField<UniqueId>(ffc.GetByName("uniqueId"));
            var portfolio = deserializer.FromField<ManageablePortfolio>(ffc.GetByName("portfolio"));

            return new PortfolioDocument(versionFromInstant, versionToInstant, correctionFromInstant, correctionToInstant, uid, portfolio);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            WriteDocumentFields(a);

            if (UniqueId != null)
            {
                a.Add("uniqueId", UniqueId.ToString());
            }
            a.Add("portfolio", Portfolio);
        }
    }
}
