//-----------------------------------------------------------------------
// <copyright file="UserMarketDataSpecification.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Engine.marketdata.spec
{
    [FudgeSurrogate(typeof(UserMarketDataSpecificationBuilder))]
    public class UserMarketDataSpecification : MarketDataSpecification
    {
        private readonly UniqueIdentifier _userSnapshotID;

        public UserMarketDataSpecification(UniqueIdentifier userSnapshotID)
        {
            _userSnapshotID = userSnapshotID;
        }

        public UniqueIdentifier UserSnapshotID { get { return _userSnapshotID; } }
    }
}