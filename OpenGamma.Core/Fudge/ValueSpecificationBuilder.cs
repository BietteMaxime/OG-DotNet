// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValueSpecificationBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Engine;
using OpenGamma.Engine.Value;

namespace OpenGamma.Fudge
{
    class ValueSpecificationBuilder : BuilderBase<ValueSpecification>
    {
        private const string ValueNameField = "valueName";
        private const string PropertiesField = "properties";

        public ValueSpecificationBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override void SerializeImpl(ValueSpecification obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            msg.Add(ValueNameField, obj.ValueName);
            ComputationTargetReferenceBuilder.SerializeCore(obj.TargetSpecification, msg, serializer);
            serializer.WriteInline(msg, "properties", obj.Properties);
        }

        protected override ValueSpecification DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            string valueName = msg.GetString(ValueNameField);
            ComputationTargetSpecification targetSpecification = ComputationTargetReferenceBuilder.DeserializeCore(msg, deserializer).Specification;
            var properties = deserializer.FromField<ValueProperties>(msg.GetByName(PropertiesField));
            return new ValueSpecification(valueName, targetSpecification, properties);
        }
    }
}
