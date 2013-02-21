// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserMarketDataSpecificationBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Engine.MarketData.Spec;
using OpenGamma.Id;

namespace OpenGamma.Fudge
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

        protected override UserMarketDataSpecification DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return new UserMarketDataSpecification(deserializer.FromField<UniqueId>(msg.GetByName("userSnapshotId")));
        }
    }
}