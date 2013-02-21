// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComputationCacheResponseBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Engine.Value;
using OpenGamma.Engine.View.Calc;
using OpenGamma.Util.Tuple;

namespace OpenGamma.Fudge
{
    class ComputationCacheResponseBuilder : BuilderBase<ComputationCacheResponse>
    {
        public ComputationCacheResponseBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override ComputationCacheResponse DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return new ComputationCacheResponse(msg.GetMessage("results").Select(f => GetValue(f, deserializer)).ToList());
        }

        private static Pair<ValueSpecification, object> GetValue(IFudgeField field, IFudgeDeserializer deserializer)
        {
            var msg = (IFudgeFieldContainer)field.Value;
            var spec = deserializer.FromField<ValueSpecification>(msg.GetByName("first"));
            var value = ComputedValueBuilder.GetValue(deserializer, msg.GetByName("second"), spec);
            return new Pair<ValueSpecification, object>(spec, value);
        }
    }
}
