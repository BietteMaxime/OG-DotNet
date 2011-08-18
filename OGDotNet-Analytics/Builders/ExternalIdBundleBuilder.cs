//-----------------------------------------------------------------------
// <copyright file="ExternalIdBundleBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Builders
{
    class ExternalIdBundleBuilder : BuilderBase<ExternalIdBundle>
    {
        public ExternalIdBundleBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override ExternalIdBundle DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var identifiers = new HashSet<ExternalId>();

            foreach (var field in msg)
            {
                switch (field.Name)
                {
                    case "ID":
                        var i = (ExternalId)deserializer.FromField(field, typeof(ExternalId));
                        identifiers.Add(i);
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
            return new ExternalIdBundle(identifiers);
        }

        protected override void SerializeImpl(ExternalIdBundle obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            foreach (var identifier in obj.Identifiers)
            {
                msg.Add("ID", identifier);
            }
        }
    }
}
