//-----------------------------------------------------------------------
// <copyright file="TradeImpl.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Core.Position.Impl
{
    [FudgeSurrogate(typeof(TradeBuilder))]
    class TradeImpl : ITrade
    {
        private readonly UniqueIdentifier _uniqueId;
        private readonly UniqueIdentifier _parentPositionId;
        private readonly DateTimeOffset _tradeDate;
        private readonly IdentifierBundle _securityKey;
        private readonly CounterpartyImpl _counterparty;
        private readonly long _quantity;

        public TradeImpl(UniqueIdentifier uniqueId, UniqueIdentifier parentPositionId, DateTimeOffset tradeDate, IdentifierBundle securityKey, CounterpartyImpl counterparty, long quantity)
        {
            _uniqueId = uniqueId;
            _quantity = quantity;
            _securityKey = securityKey;
            _counterparty = counterparty;
            _tradeDate = tradeDate;
            _parentPositionId = parentPositionId;
        }

        public UniqueIdentifier ParentPositionId
        {
            get { return _parentPositionId; }
        }

        public DateTimeOffset TradeDate
        {
            get { return _tradeDate; }
        }

        public IdentifierBundle SecurityKey
        {
            get { return _securityKey; }
        }

        public long Quantity
        {
            get { return _quantity; }
        }

        public UniqueIdentifier UniqueId
        {
            get { return _uniqueId; }
        }

        public ICounterparty Counterparty
        {
            get { return _counterparty; }
        }
    }
}
