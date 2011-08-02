//-----------------------------------------------------------------------
// <copyright file="RemoveValueRequest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using OGDotNet.Mappedtypes.Engine.Value;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Financial.LiveData.Rest
{
    public class RemoveValueRequest
    {
        private ValueRequirement _valueRequirement;
        private Identifier _identifier;
        private string _valueName;

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
    }
}