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
using OGDotNet.Mappedtypes.engine.marketdata.spec;

namespace OGDotNet.Builders
{
    class UserMarketDataSpecificationBuilder : BuilderBase<UserMarketDataSpecification>
    {
        public UserMarketDataSpecificationBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override void SerializeImpl(UserMarketDataSpecification obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            serializer.WriteTypeHeader(msg, obj.GetType());
            serializer.WriteInline(msg, "userSnapshotId", obj.UserSnapshotID);
        }

        public override UserMarketDataSpecification DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            throw new NotImplementedException();
        }
    }
    class LiveMarketDataSpecificationBuilder : BuilderBase<LiveMarketDataSpecification>
    {
        public LiveMarketDataSpecificationBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override void SerializeImpl(LiveMarketDataSpecification obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            serializer.WriteTypeHeader(msg, obj.GetType());
            msg.Add("dataSource", obj.DataSource);
        }

        public override LiveMarketDataSpecification DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            throw new NotImplementedException();
        }
    }
}
