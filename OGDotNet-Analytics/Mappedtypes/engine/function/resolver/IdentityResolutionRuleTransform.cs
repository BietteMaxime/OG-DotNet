//-----------------------------------------------------------------------
// <copyright file="IdentityResolutionRuleTransform.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Mappedtypes.engine.function.resolver
{
    public class IdentityResolutionRuleTransform : IResolutionRuleTransform
    {
        public static readonly IdentityResolutionRuleTransform Instance = new IdentityResolutionRuleTransform();

        private IdentityResolutionRuleTransform()
        {
        }

        public static IdentityResolutionRuleTransform FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return Instance;
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
        }
    }
}