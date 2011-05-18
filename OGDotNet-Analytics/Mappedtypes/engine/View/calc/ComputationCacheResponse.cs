//-----------------------------------------------------------------------
// <copyright file="ComputationCacheResponse.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using OGDotNet.Mappedtypes.engine.Value;
using OGDotNet.Mappedtypes.Util.tuple;

namespace OGDotNet.Mappedtypes.engine.View.calc
{
    public class ComputationCacheResponse
    {
        private readonly IList<Pair<ValueSpecification, Object>> _results;

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
