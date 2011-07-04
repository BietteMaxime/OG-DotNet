//-----------------------------------------------------------------------
// <copyright file="UserMarketDataSpecificationBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.engine.marketdata.spec;
using OGDotNet.Mappedtypes.Id;

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
            return new UserMarketDataSpecification(deserializer.FromField<UniqueIdentifier>(msg.GetByName("userSnapshotId")));
        }
    }
}