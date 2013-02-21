// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExternalIdBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Id;

namespace OpenGamma.Fudge
{
    internal class ExternalIdBuilder : BuilderBase<ExternalId>
    {
        public ExternalIdBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override ExternalId DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            string scheme = msg.GetValue<string>("Scheme");
            string value = msg.GetValue<string>("Value");
            return new ExternalId(scheme, value);
        }

        protected override void SerializeImpl(ExternalId obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            msg.Add("Scheme", obj.Scheme);
            msg.Add("Value", obj.Value);
        }
    }
}
