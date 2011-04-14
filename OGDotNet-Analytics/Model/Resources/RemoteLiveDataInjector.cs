//-----------------------------------------------------------------------
// <copyright file="RemoteLiveDataInjector.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using Fudge;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.value;
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

            FudgeMsg msg = new FudgeMsg(new Field("value", value));
            GetValueReqTarget(valueRequirement).Put(msg);
        }

        public void RemoveValue(ValueRequirement valueRequirement)
        {
            ArgumentChecker.NotNull(valueRequirement, "valueRequirement");
            var valueReqTarget = GetValueReqTarget(valueRequirement);
            valueReqTarget.Delete();
        }

        private RestTarget GetValueReqTarget(ValueRequirement valueRequirement)
        {
            return _rest.Resolve(
                valueRequirement.ValueName,
                EnumBuilder<ComputationTargetType>.GetJavaName(valueRequirement.TargetSpecification.Type),
                valueRequirement.TargetSpecification.Uid.ToString());
        }
    }
}