// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractLinkBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;
using OpenGamma.Id;
using OpenGamma.Master;

namespace OpenGamma.Fudge
{
    internal abstract class AbstractLinkBuilder<T> : BuilderBase<AbstractLink<T>>
    {
        protected AbstractLinkBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected abstract AbstractLink<T> CreateLink();

        protected override AbstractLink<T> DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            AbstractLink<T> link = CreateLink();
            IFudgeField externalIdField = msg.GetByName("externalId");
            link.ExternalId = externalIdField != null ? deserializer.FromField<ExternalIdBundle>(externalIdField) : null;
            IFudgeField objectIdField = msg.GetByName("objectId");
            link.ObjectId = objectIdField != null ? deserializer.FromField<ObjectId>(objectIdField) : null;
            return link;
        }

        protected override void SerializeImpl(AbstractLink<T> obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            serializer.WriteInline(msg, "externalId", obj.ExternalId);
            serializer.WriteInline(msg, "objectId", obj.ObjectId);
        }
    }
}