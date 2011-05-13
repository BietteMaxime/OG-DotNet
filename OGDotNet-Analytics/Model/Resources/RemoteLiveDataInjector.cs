//-----------------------------------------------------------------------
// <copyright file="RemoteLiveDataInjector.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.financial.livedata.rest;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    public class RemoteLiveDataInjector
    {
        private readonly RestTarget _rest;

        public RemoteLiveDataInjector(RestTarget rest)
        {
            _rest = rest;
        }

        public void AddValue(ValueRequirement valueRequirement, object value)
        {
            ArgumentChecker.NotNull(valueRequirement, "valueRequirement");
            var addValueRequest = new AddValueRequest { Value = value, ValueRequirement = valueRequirement };
            AddValue(addValueRequest);
        }

        public void AddValue(Identifier identifier, string valueName, object value)
        {
            ArgumentChecker.NotNull(identifier, "identifier");
            ArgumentChecker.NotEmpty(valueName, "valueName");
            AddValue(new AddValueRequest { Identifier = identifier, ValueName = valueName, Value = value});
        }


        private void AddValue(AddValueRequest addValueRequest)
        {
            _rest.Resolve("add").Post(addValueRequest);
        }

        public void RemoveValue(Identifier identifier, string valueName)
        {
            ArgumentChecker.NotNull(identifier, "identifier");
            ArgumentChecker.NotEmpty(valueName, "valueName");
            RemoveValue(new RemoveValueRequest {Identifier = identifier, ValueName = valueName});
        }

        public void RemoveValue(ValueRequirement valueRequirement)
        {
            ArgumentChecker.NotNull(valueRequirement, "valueRequirement");
            RemoveValue(new RemoveValueRequest { ValueRequirement = valueRequirement });
        }

        private void RemoveValue(RemoveValueRequest removeValueRequest)
        {
            _rest.Resolve("remove").Post(removeValueRequest);
        }
    }
}