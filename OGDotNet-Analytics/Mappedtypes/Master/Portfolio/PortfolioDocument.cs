//-----------------------------------------------------------------------
// <copyright file="PortfolioDocument.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Master.Portfolio
{
    [FudgeSurrogate(typeof(PortfolioDocumentBuilder))]
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
    }
}
