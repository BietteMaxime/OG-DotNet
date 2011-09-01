//-----------------------------------------------------------------------
// <copyright file="LiveMarketDataSpecificationBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Engine.MarketData.Spec;

namespace OGDotNet.Builders
{
    class LiveMarketDataSpecificationBuilder : BuilderBase<LiveMarketDataSpecification>
    {
        public LiveMarketDataSpecificationBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override void SerializeImpl(LiveMarketDataSpecification obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            serializer.WriteTypeHeader(msg, obj.GetType());
            if (obj.DataSource != null)
            {
                msg.Add("dataSource", obj.DataSource);
            }
        }

        public override LiveMarketDataSpecification DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return new LiveMarketDataSpecification(msg.GetString("dataSource"));
        }
    }
}
