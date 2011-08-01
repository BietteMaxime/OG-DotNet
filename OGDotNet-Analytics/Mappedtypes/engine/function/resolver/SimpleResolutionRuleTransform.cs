//-----------------------------------------------------------------------
// <copyright file="SimpleResolutionRuleTransform.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Mappedtypes.engine.function.resolver
{
    /// <summary>
    /// TODO: this
    /// </summary>
    public class SimpleResolutionRuleTransform : IResolutionRuleTransform
    {
        private readonly IFudgeFieldContainer _fields;

        public SimpleResolutionRuleTransform(IFudgeFieldContainer fields)
        {
            _fields = fields;
        }

        public static SimpleResolutionRuleTransform FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new SimpleResolutionRuleTransform(ffc);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            foreach (var field in _fields)
            {
                a.Add(field);
            }
        }
    }
}