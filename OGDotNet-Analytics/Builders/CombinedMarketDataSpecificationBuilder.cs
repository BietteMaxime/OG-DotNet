//-----------------------------------------------------------------------
// <copyright file="CombinedMarketDataSpecificationBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Engine.marketdata.spec;

namespace OGDotNet.Builders
{
    class CombinedMarketDataSpecificationBuilder : BuilderBase<CombinedMarketDataSpecification>
    {
        public CombinedMarketDataSpecificationBuilder(FudgeContext context, Type type)
            : base(context, type)
        {
        }

        protected override void SerializeImpl(CombinedMarketDataSpecification obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            serializer.WriteTypeHeader(msg, obj.GetType());
            serializer.WriteInline(msg, "prefferedSpecification", obj.PrefferedSpecification);
            serializer.WriteInline(msg, "fallbackSpecification", obj.FallbackSpecification);
        }

        public override CombinedMarketDataSpecification DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return new CombinedMarketDataSpecification(deserializer.FromField<MarketDataSpecification>(msg.GetByName("prefferedSpecification")),
                                                       deserializer.FromField<MarketDataSpecification>(msg.GetByName("fallbackSpecification")));
        }
    }
}