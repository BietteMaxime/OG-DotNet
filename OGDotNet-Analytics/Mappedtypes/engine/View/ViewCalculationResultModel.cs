//-----------------------------------------------------------------------
// <copyright file="ViewCalculationResultModel.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using OGDotNet.Mappedtypes.Engine.value;

namespace OGDotNet.Mappedtypes.Engine.View
{
    public class ViewCalculationResultModel
    {
        private readonly Dictionary<ComputationTargetSpecification, ISet<ComputedValue>> _mapAll;

        public ViewCalculationResultModel(IDictionary<ComputationTargetSpecification, IDictionary<string, ComputedValue>> map, Dictionary<ComputationTargetSpecification, ISet<ComputedValue>> mapAll)
        {
            _mapAll = mapAll;
        }

        public IEnumerable<ComputedValue> AllResults
        {
            get
            {
                return _mapAll.SelectMany(kvp => kvp.Value);
            }
        }

        public bool TryGetAllValues(ComputationTargetSpecification target, out ISet<ComputedValue> values)
        {
            return _mapAll.TryGetValue(target, out values);
        }
    }
}