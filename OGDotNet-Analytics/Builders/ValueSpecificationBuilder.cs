//-----------------------------------------------------------------------
// <copyright file="ValueSpecificationBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.Value;

namespace OGDotNet.Builders
{
    class ValueSpecificationBuilder : BuilderBase<ValueSpecification>
    {
        public ValueSpecificationBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override ValueSpecification DeserializeImpl(Fudge.IFudgeFieldContainer msg, Fudge.Serialization.IFudgeDeserializer deserializer)
        {
            string valueName = null;
            ValueProperties properties = null;

            foreach (var fudgeField in msg.GetAllFields())
            {
                switch (fudgeField.Name)
                {
                    case "valueName":
                        valueName = (string) fudgeField.Value;
                        break;
                    case "properties":
                        properties = deserializer.FromField<ValueProperties>(fudgeField);
                        break;
                    default:
                        break;
                }
            }
            
            var targetSpecification = new ComputationTargetSpecificationBuilder(deserializer.Context, typeof(ComputationTargetSpecification)).DeserializeImpl(msg, deserializer); //Can't register twice

            return new ValueSpecification(valueName, targetSpecification, properties);
        }
    }
}
