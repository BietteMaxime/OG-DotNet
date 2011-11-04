//-----------------------------------------------------------------------
// <copyright file="ManageablePortfolio.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Master.Portfolio
{
    [FudgeSurrogate(typeof(ManageablePortfolioBuilder))]
    public class ManageablePortfolio : IUniqueIdentifiable
    {
        //TODO this
        private readonly string _name;
        private readonly UniqueId _uniqueId;

        public ManageablePortfolio(string name, UniqueId uniqueId)
        {
            _name = name;
            _uniqueId = uniqueId;
        }

        public string Name
        {
            get { return _name; }
        }

        public UniqueId UniqueId
        {
            get { return _uniqueId; }
        }
    }
}