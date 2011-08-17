//-----------------------------------------------------------------------
// <copyright file="AvailableOutputsImplBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Engine.Value;
using OGDotNet.Mappedtypes.Engine.View.Helper;

namespace OGDotNet.Builders
{
    class AvailableOutputsImplBuilder : BuilderBase<AvailableOutputsImpl>
    {
        public AvailableOutputsImplBuilder(FudgeContext context, Type type)
            : base(context, type)
        {
        }

        public override AvailableOutputsImpl DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var securityTypes = new HashSet<string>();
            var outputsByValueName = new Dictionary<string, AvailableOutput>();
            
            foreach (var typeField in msg)
            {
                if (!(typeField.Value is FudgeMsg))
                {
                    continue;
                }
                FudgeMsg submsg = (FudgeMsg) typeField.Value;
                foreach (var valueField in submsg)
                {
                    AvailableOutput availableOutput;
                    if (!outputsByValueName.TryGetValue(valueField.Name, out availableOutput))
                    {
                        availableOutput = new AvailableOutput(valueField.Name);
                        outputsByValueName.Add(valueField.Name, availableOutput);
                    }

                    var valueProperties = deserializer.FromField<ValueProperties>(valueField);

                    if (typeField.Name == null)
                    {
                        if (availableOutput.PortfolioNodeProperties != null)
                        {
                            throw new ArgumentException("Duplicate portfolio node properties");
                        }
                        availableOutput.PortfolioNodeProperties = valueProperties;
                    }
                    else
                    {
                        securityTypes.Add(typeField.Name);
                        availableOutput.PositionProperties.Add(typeField.Name, valueProperties);
                    }
                }
            }

            return new AvailableOutputsImpl(securityTypes, outputsByValueName);
        }
    }
}
