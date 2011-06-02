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
            var uniqueIdentifier = UniqueIdentifier.Parse(ffc.GetString("uniqueId"));

            var ppIdField = ffc.GetString("parentPositionId");
            var parentPositionId = ppIdField == null ? null : UniqueIdentifier.Parse(ppIdField);
            
            var tradeDate = ffc.GetValue<DateTimeOffset>("tradeDate");

            var securityKey = deserializer.FromField<IdentifierBundle>(ffc.GetByName("securityKey"));

            var counterPartyIdentifier = Identifier.Parse(ffc.GetString("counterparty"));
            var quant = ffc.GetValue<long>("quantity");
            return new TradeImpl(uniqueIdentifier, parentPositionId, tradeDate, securityKey, new CounterpartyImpl(counterPartyIdentifier), quant);
        }
    }
}
