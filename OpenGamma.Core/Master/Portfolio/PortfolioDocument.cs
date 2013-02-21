// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PortfolioDocument.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge.Serialization;

using OpenGamma.Fudge;
using OpenGamma.Id;

namespace OpenGamma.Master.Portfolio
{
    [FudgeSurrogate(typeof(PortfolioDocumentBuilder))]
    public class PortfolioDocument : AbstractDocument
    {
        private readonly ManageablePortfolio _portfolio;

        public PortfolioDocument(ManageablePortfolio portfolio)
            : base(default(DateTimeOffset), default(DateTimeOffset), default(DateTimeOffset), default(DateTimeOffset))
        {
            _portfolio = portfolio;
        }

        public PortfolioDocument(DateTimeOffset versionFromInstant, DateTimeOffset versionToInstant, DateTimeOffset correctionFromInstant, DateTimeOffset correctionToInstant, UniqueId uniqueId, ManageablePortfolio portfolio)
            : base(versionFromInstant, versionToInstant, correctionFromInstant, correctionToInstant)
        {
            UniqueId = uniqueId;
            _portfolio = portfolio;
        }

        public override UniqueId UniqueId { get; set; }

        public ManageablePortfolio Portfolio
        {
            get { return _portfolio; }
        }
    }
}
