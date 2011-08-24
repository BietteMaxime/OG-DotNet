//-----------------------------------------------------------------------
// <copyright file="PagingBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Util;

namespace OGDotNet.Builders
{
    class PagingBuilder : BuilderBase<Paging>
    {
        public PagingBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override Paging DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return new Paging(PagingRequest.OfIndex(msg.GetInt("first").Value, msg.GetInt("size").Value), msg.GetInt("total").Value);
        }
    }
}
