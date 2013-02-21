// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComputationTargetReferenceBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;
using OpenGamma.Engine;
using OpenGamma.Engine.Target;
using OpenGamma.Id;

namespace OpenGamma.Fudge
{
    internal class ComputationTargetReferenceBuilder : BuilderBase<ComputationTargetReference>
    {
        private const string IdentifierField = "ComputationTargetIdentifier";
        private static readonly ComputationTargetReferenceIdentifierVisitor IdentifierVisitor = new ComputationTargetReferenceIdentifierVisitor(); 

        public ComputationTargetReferenceBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        private static void SerializeIds(ComputationTargetReference obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            if (obj.Parent != null)
            {
                SerializeIds(obj.Parent, msg, serializer);
            }
            serializer.WriteInline(msg, IdentifierField, obj.Accept(IdentifierVisitor));
        }

        public static void SerializeCore(ComputationTargetReference obj, IAppendingFudgeFieldContainer msg,
                           IFudgeSerializer serializer)
        {
            ComputationTargetTypeBuilder.SerializeCore(obj.Type, msg, serializer);
            SerializeIds(obj, msg, serializer);
        }

        protected override void SerializeImpl(ComputationTargetReference obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            SerializeCore(obj, msg, serializer);
        }

        public static ComputationTargetReference DeserializeCore(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            ComputationTargetType targetType = ComputationTargetTypeBuilder.DeserializeCore(msg, deserializer);
            IFudgeField identifierField = msg.GetByName(IdentifierField);
            if (targetType is NullComputationTargetType)
            {
                return identifierField == null
                           ? (ComputationTargetReference) ComputationTargetSpecification.Null
                           : new ComputationTargetRequirement(ComputationTargetType.Null, ExternalIdBundle.Empty());
            }
            if (targetType is NestedComputationTargetType)
            {
                ComputationTargetReference result = null;
                foreach (IFudgeField idField in msg.GetAllByName(IdentifierField))
                {
                    if (idField.Value is FudgeMsg)
                    {
                        var identifiers = deserializer.FromField<ExternalIdBundle>(idField);
                        result = result == null
                                     ? new ComputationTargetRequirement(targetType, identifiers)
                                     : result.Containing(targetType, identifiers);
                    }
                    else
                    {
                        var identifier = deserializer.FromField<UniqueId>(identifierField);
                        result = result == null
                                     ? new ComputationTargetSpecification(targetType, identifier)
                                     : result.Containing(targetType, identifier);
                    }
                }
                return result;
            }
            return identifierField.Value is FudgeMsg
                       ? (ComputationTargetReference)
                         new ComputationTargetRequirement(targetType, deserializer.FromField<ExternalIdBundle>(identifierField))
                       : new ComputationTargetSpecification(targetType, deserializer.FromField<UniqueId>(identifierField));
        }

        protected override ComputationTargetReference DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return DeserializeCore(msg, deserializer);
        }
    }
}