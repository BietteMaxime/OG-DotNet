//-----------------------------------------------------------------------
// <copyright file="UnorderedCurrencyPairBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Util.Money;

namespace OGDotNet.Builders
{
    internal class UnorderedCurrencyPairBuilder : BuilderBase<UnorderedCurrencyPair>
    {
        public UnorderedCurrencyPairBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override UnorderedCurrencyPair DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            Currency ccy1 = Currency.Create(msg.GetString("currency1"));
            Currency ccy2 = Currency.Create(msg.GetString("currency2"));
            return new UnorderedCurrencyPair(ccy1, ccy2);
        }
    }
}
