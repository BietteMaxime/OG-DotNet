//-----------------------------------------------------------------------
// <copyright file="RemoteLiveDataInjector.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.financial.livedata.rest;
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

        public void AddValue(ValueRequirement valueRequirement, double value)
        {
            ArgumentChecker.NotNull(valueRequirement, "valueRequirement");
            var addValueRequest = new AddValueRequest { Value = value, ValueRequirement = valueRequirement };
            AddValue(addValueRequest);
        }

        private void AddValue(AddValueRequest addValueRequest)
        {
            _rest.Resolve("add").Post(addValueRequest);
        }

        public void RemoveValue(ValueRequirement valueRequirement)
        {
            ArgumentChecker.NotNull(valueRequirement, "valueRequirement");
            var removeValueRequest = new RemoveValueRequest { ValueRequirement = valueRequirement };
            RemoveValue(removeValueRequest);
        }

        private void RemoveValue(RemoveValueRequest removeValueRequest)
        {
            _rest.Resolve("remove").Post(removeValueRequest);
        }
    }
}