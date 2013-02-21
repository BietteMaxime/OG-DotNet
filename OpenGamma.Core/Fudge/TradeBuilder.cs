// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TradeBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Core.Position;
using OpenGamma.Core.Position.Impl;
using OpenGamma.Id;

namespace OpenGamma.Fudge
{
    class TradeBuilder : BuilderBase<ITrade>
    {
        public TradeBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override ITrade DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            // TODO: the rest of this
            var uniqueIdentifier = UniqueId.Parse(msg.GetString("uniqueId"));

            var tradeDate = msg.GetValue<DateTimeOffset>("tradeDate");

            var securityKey = deserializer.FromField<ExternalIdBundle>(msg.GetByName("securityKey"));

            var counterPartyIdentifier = ExternalId.Parse(msg.GetString("counterpartyKey") ?? msg.GetString("counterparty")); // NOTE: this is a hack because we don't use proto yet
            var quant = msg.GetValue<string>("quantity");
            return new SimpleTrade(uniqueIdentifier, tradeDate, securityKey, new CounterpartyImpl(counterPartyIdentifier), decimal.Parse(quant));
        }
    }
}
