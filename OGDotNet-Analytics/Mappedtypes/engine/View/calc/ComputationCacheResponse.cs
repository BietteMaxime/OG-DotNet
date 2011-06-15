//-----------------------------------------------------------------------
// <copyright file="ComputationCacheResponse.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.Util.tuple;

namespace OGDotNet.Mappedtypes.engine.View.calc
{
    public class ComputationCacheResponse
    {
        private readonly IList<Pair<ValueSpecification, object>> _results;

        public ComputationCacheResponse(IList<Pair<ValueSpecification, object>> results)
        {
            _results = results;
        }

        public IList<Pair<ValueSpecification, object>> Results
        {
            get { return _results; }
        }

        public static ComputationCacheResponse FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new ComputationCacheResponse(ffc.GetMessage("results").Select(f => GetValue(f, deserializer)).ToList());
        }
        private static Pair<ValueSpecification, object> GetValue(IFudgeField field, IFudgeDeserializer deserializer)
        {
            var msg = (IFudgeFieldContainer) field.Value;
            var spec = deserializer.FromField<ValueSpecification>(msg.GetByName("first"));
            var value = ComputedValueBuilder.GetValue(deserializer, msg.GetByName("second"), spec);
            return new Pair<ValueSpecification, object>(spec, value);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}
