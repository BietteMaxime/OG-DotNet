//-----------------------------------------------------------------------
// <copyright file="AvailableOutput.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using OGDotNet.Mappedtypes.Engine.Value;

namespace OGDotNet.Mappedtypes.Engine.View.Helper
{
    public class AvailableOutput
    {
        private readonly string _valueName;
        private readonly Dictionary<string, ValueProperties> _positionProperties = new Dictionary<string, ValueProperties>();

        public AvailableOutput(string valueName)
        {
            _valueName = valueName;
        }

        public string ValueName
        {
            get { return _valueName; }
        }

        public Dictionary<string, ValueProperties> PositionProperties
        {
            get { return _positionProperties; }
        }

        public ValueProperties PortfolioNodeProperties { get; set; }
    }
}