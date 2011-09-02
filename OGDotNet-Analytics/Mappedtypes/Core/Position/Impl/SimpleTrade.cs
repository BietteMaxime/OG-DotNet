//-----------------------------------------------------------------------
// <copyright file="SimpleTrade.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
    class SimpleTrade : ITrade
    {
        private readonly UniqueId _uniqueId;
        private readonly DateTimeOffset _tradeDate;
        private readonly ExternalIdBundle _securityKey;
        private readonly CounterpartyImpl _counterparty;
        private readonly decimal _quantity;

        public SimpleTrade(UniqueId uniqueId, DateTimeOffset tradeDate, ExternalIdBundle securityKey, CounterpartyImpl counterparty, decimal quantity)
        {
            _uniqueId = uniqueId;
            _quantity = quantity;
            _securityKey = securityKey;
            _counterparty = counterparty;
            _tradeDate = tradeDate;
        }

        public DateTimeOffset TradeDate
        {
            get { return _tradeDate; }
        }

        public ExternalIdBundle SecurityKey
        {
            get { return _securityKey; }
        }

        public decimal Quantity
        {
            get { return _quantity; }
        }

        public UniqueId UniqueId
        {
            get { return _uniqueId; }
        }

        public ICounterparty Counterparty
        {
            get { return _counterparty; }
        }
    }
}
