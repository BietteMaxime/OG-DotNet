//-----------------------------------------------------------------------
// <copyright file="ViewCalculationResultModel.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using OGDotNet.Mappedtypes.engine.Value;

namespace OGDotNet.Mappedtypes.engine.View
{
    public class ViewCalculationResultModel
    {
        private readonly IDictionary<ComputationTargetSpecification, IDictionary<string, ComputedValue>> _map;
        private readonly Dictionary<ComputationTargetSpecification, ISet<ComputedValue>> _mapAll;

        public ViewCalculationResultModel(IDictionary<ComputationTargetSpecification, IDictionary<string, ComputedValue>> map, Dictionary<ComputationTargetSpecification, ISet<ComputedValue>> mapAll)
        {
            _map = map;
            _mapAll = mapAll;
        }

        public IEnumerable<ComputationTargetSpecification> AllTargets
        {
            get { return _map.Keys; }
        }

        public IEnumerable<ComputedValue> AllResults
        {
            get
            {
                return _mapAll.SelectMany(kvp => kvp.Value);
            }
        }

        
        /// <summary>
        /// NOTE:  If multiple values were produced for a given value name, an arbitrary choice is made for which to include in the map.  
        /// </summary>
        /// <param name="target"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public bool TryGetValues(ComputationTargetSpecification target, out IDictionary<string, ComputedValue> values)
        {
            return _map.TryGetValue(target, out values);
        }

        /// <summary>
        /// /// NOTE:  If multiple values were produced for a given value name, an arbitrary choice is made for which to include in the map.  
        /// </summary>
        /// <param name="target"></param>
        /// <param name="valueName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(ComputationTargetSpecification target, string valueName, out ComputedValue value)
        {
            IDictionary<string, ComputedValue> valueDict;
            if (!TryGetValues(target, out valueDict))
            {
                value = null;
                return false;
            }
            return valueDict.TryGetValue(valueName, out value);
        }

        public bool TryGetAllValues(ComputationTargetSpecification target, out ISet<ComputedValue> values)
        {
            return _mapAll.TryGetValue(target, out values);
        }
    }
}