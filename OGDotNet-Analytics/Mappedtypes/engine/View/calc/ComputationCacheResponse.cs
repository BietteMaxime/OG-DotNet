//-----------------------------------------------------------------------
// <copyright file="ComputationCacheResponse.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Engine.Value;
using OGDotNet.Mappedtypes.Util.Tuple;

namespace OGDotNet.Mappedtypes.Engine.View.Calc
{
    [FudgeSurrogate(typeof(ComputationCacheResponseBuilder))]
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
    }
}
