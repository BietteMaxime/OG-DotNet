// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimplePosition.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using Fudge.Serialization;

using OpenGamma.Fudge;
using OpenGamma.Id;

namespace OpenGamma.Core.Position.Impl
{
    [FudgeSurrogate(typeof(SimplePositionBuilder))]
    class SimplePosition : IPosition
    {
        private readonly ExternalIdBundle _securityKey;
        private readonly IList<ITrade> _trades;
        private readonly UniqueId _identifier;
        private readonly decimal _quantity;

        public SimplePosition(UniqueId identifier, decimal quantity, ExternalIdBundle securityKey, IList<ITrade> trades)
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

        public decimal Quantity
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