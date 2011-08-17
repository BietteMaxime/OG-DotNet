//-----------------------------------------------------------------------
// <copyright file="AvailableOutputsImpl.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Engine.Value;

namespace OGDotNet.Mappedtypes.Engine.View.Helper
{
    [FudgeSurrogate(typeof(AvailableOutputsImplBuilder))]
    internal class AvailableOutputsImpl : IAvailableOutputs
    {
        private readonly ICollection<string> _securityTypes;
        private readonly Dictionary<string, AvailableOutput> _outputsByValueName;

        public AvailableOutputsImpl(ICollection<string> securityTypes, Dictionary<string, AvailableOutput> outputsByValueName)
        {
            _securityTypes = securityTypes;
            _outputsByValueName = outputsByValueName;
        }

        public ICollection<string> SecurityTypes
        {
            get { return _securityTypes; }
        }

        public ICollection<AvailableOutput> GetPositionOutputs(string securityType)
        {
            var ret = new HashSet<AvailableOutput>();
            foreach (var availableOutput in _outputsByValueName)
            {
                ValueProperties props;
                if (availableOutput.Value.PositionProperties.TryGetValue(securityType, out props))
                {
                    var output = new AvailableOutput(availableOutput.Key);
                    output.PositionProperties.Add(securityType, props);
                    ret.Add(output);
                }
            }
            return ret;
        }

        public ICollection<AvailableOutput> GetPortfolioNodeOutputs()
        {
            var ret = new HashSet<AvailableOutput>();
            foreach (var availableOutput in _outputsByValueName)
            {
                if (availableOutput.Value.PortfolioNodeProperties != null)
                {
                    var output = new AvailableOutput(availableOutput.Key)
                                     {PortfolioNodeProperties = availableOutput.Value.PortfolioNodeProperties};
                    ret.Add(output);
                }
            }
            return ret;
        }
    }
}