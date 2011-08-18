//-----------------------------------------------------------------------
// <copyright file="ComputationCacheQueryBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Engine.View.Calc;
using OGDotNet.Model;

namespace OGDotNet.Builders
{
    public class ComputationCacheQueryBuilder : BuilderBase<ComputationCacheQuery>
    {
        public ComputationCacheQueryBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override void SerializeImpl(ComputationCacheQuery obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer s)
        {
            var fudgeContext = s.Context;

            msg.Add("calculationConfigurationName", obj.CalculationConfigurationName);
            var fudgeMsg = new FudgeMsg(fudgeContext);
            var s2 = ((OpenGammaFudgeContext)fudgeContext).GetSerializer();
            foreach (var valueSpecification in obj.ValueSpecifications)
            {
                fudgeMsg.Add(null, null, s2.SerializeToMsg(valueSpecification));
            }
            msg.Add("valueSpecifications", fudgeMsg);
        }

        public override ComputationCacheQuery DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            throw new NotImplementedException();
        }
    }
}
