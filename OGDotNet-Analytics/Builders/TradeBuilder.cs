//-----------------------------------------------------------------------
// <copyright file="TradeBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.Core.Position.Impl;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Builders
{
    class TradeBuilder : BuilderBase<ITrade>
    {
        public TradeBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override ITrade DeserializeImpl(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            //TODO: the rest of this
            var uniqueIdentifier = UniqueId.Parse(ffc.GetString("uniqueId"));

            var tradeDate = ffc.GetValue<DateTimeOffset>("tradeDate");

            var securityKey = deserializer.FromField<ExternalIdBundle>(ffc.GetByName("securityKey"));

            var counterPartyIdentifier = ExternalId.Parse(ffc.GetString("counterpartyKey") ?? ffc.GetString("counterparty")); //NOTE: this is a hack because we don't use proto yet
            var quant = ffc.GetValue<long>("quantity");
            return new SimpleTrade(uniqueIdentifier, tradeDate, securityKey, new CounterpartyImpl(counterPartyIdentifier), quant);
        }
    }
}
