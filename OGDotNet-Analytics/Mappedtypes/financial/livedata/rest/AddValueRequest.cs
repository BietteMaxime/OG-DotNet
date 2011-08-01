//-----------------------------------------------------------------------
// <copyright file="AddValueRequest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using OGDotNet.Mappedtypes.Engine.value;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.financial.livedata.rest
{
    public class AddValueRequest
    {
        private ValueRequirement _valueRequirement;
        private Identifier _identifier;
        private string _valueName;
        private object _value;

        public ValueRequirement ValueRequirement
        {
            get { return _valueRequirement; }
            set { _valueRequirement = value; }
        }

        public Identifier Identifier
        {
            get { return _identifier; }
            set { _identifier = value; }
        }

        public string ValueName
        {
            get { return _valueName; }
            set { _valueName = value; }
        }

        public object Value
        {
            get { return _value; }
            set { _value = value; }
        }
    }
}
