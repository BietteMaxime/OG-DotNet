//-----------------------------------------------------------------------
// <copyright file="PositionImpl.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Core.Position.Impl
{
    [FudgeSurrogate(typeof(PositionBuilder))]
    class PositionImpl : IPosition
    {
        private readonly ExternalIdBundle _securityKey;
        private readonly IList<ITrade> _trades;
        private readonly UniqueId _identifier;
        private readonly long _quantity;

        public PositionImpl(UniqueId identifier, long quantity, ExternalIdBundle securityKey, IList<ITrade> trades)
        {
            _securityKey = securityKey;
            _trades = trades;
            _identifier = identifier;
            _quantity = quantity;
        }

        public ExternalIdBundle SecurityKey
        {
            get { return _securityKey; }
        }

        public long Quantity
        {
            get { return _quantity; }
        }

        public IEnumerable<ITrade> Trades
        {
            get { return _trades; }
        }

        public UniqueId UniqueId
        {
            get { return _identifier; }
        }
    }
}
