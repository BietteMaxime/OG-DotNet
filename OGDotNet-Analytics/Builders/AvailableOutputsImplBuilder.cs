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
                    var valueName = valueField.Name;

                    AvailableOutput availableOutput;
                    if (!outputsByValueName.TryGetValue(valueName, out availableOutput))
                    {
                        availableOutput = new AvailableOutput(valueName);
                        outputsByValueName.Add(valueName, availableOutput);
                    }

                    var valueProperties = deserializer.FromField<ValueProperties>(valueField);

                    var type = typeField.Name;
                    if (type == null)
                    {
                        if (availableOutput.PortfolioNodeProperties != null)
                        {
                            throw new ArgumentException("Duplicate portfolio node properties");
                        }
                        availableOutput.PortfolioNodeProperties = valueProperties;
                    }
                    else
                    {
                        securityTypes.Add(type);
                        availableOutput.PositionProperties.Add(type, valueProperties);
                    }
                }
            }

            return new AvailableOutputsImpl(securityTypes, outputsByValueName);
        }
    }
}
