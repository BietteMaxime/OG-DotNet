//-----------------------------------------------------------------------
// <copyright file="PagingRequestBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Util.Db;

namespace OGDotNet.Builders
{
    class PagingRequestBuilder : BuilderBase<PagingRequest>
    {
        public PagingRequestBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override PagingRequest DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return PagingRequest.OfIndex(msg.GetInt("first").Value, msg.GetInt("size").Value);
        }

        protected override void SerializeImpl(PagingRequest obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            msg.Add("first", obj.Index);
            msg.Add("size", obj.Size);
        }
    }
}
