using System.IO;
using Fudge;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.engine.View;
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

            FudgeMsg msg = new FudgeMsg(new Field("value",value));
            GetValueReqTarget(valueRequirement).Put(FudgeConfig.GetFudgeContext(), msg);
        }

        public void RemoveValue(ValueRequirement valueRequirement)
        {
            ArgumentChecker.NotNull(valueRequirement, "valueRequirement");
            var valueReqTarget = GetValueReqTarget(valueRequirement);
            valueReqTarget.Delete();
        }

        private RestTarget GetValueReqTarget(ValueRequirement valueRequirement)
        {
            return _rest.Resolve(GetValueRequirementSubPath(valueRequirement));
        }

        private static string GetValueRequirementSubPath(ValueRequirement valueRequirement)
        {
            return Path.Combine(valueRequirement.ValueName, ComputationTargetTypeBuilder.GetJavaName(valueRequirement.TargetSpecification.Type), valueRequirement.TargetSpecification.Uid.ToString());
        }

        private class PutDouble
        {
            public double Value
            {
                get;
                set;
            }
        }
    }
}