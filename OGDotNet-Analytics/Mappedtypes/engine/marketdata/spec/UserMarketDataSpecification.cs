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

namespace OGDotNet.Mappedtypes.Engine.MarketData.Spec
{
    [FudgeSurrogate(typeof(UserMarketDataSpecificationBuilder))]
    public class UserMarketDataSpecification : MarketDataSpecification
    {
        private readonly UniqueId _userSnapshotID;

        public UserMarketDataSpecification(UniqueId userSnapshotID)
        {
            _userSnapshotID = userSnapshotID;
        }

        public UniqueId UserSnapshotID { get { return _userSnapshotID; } }
    }
}