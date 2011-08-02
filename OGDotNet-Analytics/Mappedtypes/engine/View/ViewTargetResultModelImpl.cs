//-----------------------------------------------------------------------
// <copyright file="ViewTargetResultModelImpl.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using OGDotNet.Mappedtypes.Engine.Value;

namespace OGDotNet.Mappedtypes.Engine.View
{
    internal class ViewTargetResultModelImpl : IViewTargetResultModel
    {
        private readonly Dictionary<string, Dictionary<string, ComputedValue>> _inner = new Dictionary<string, Dictionary<string, ComputedValue>>();
        public void AddAll(string key, Dictionary<string, ComputedValue> values)
        {
            _inner.Add(key, values);
        }

        public IEnumerable<string> CalculationConfigurationNames
        {
            get { return _inner.Keys; }
        }

        public IDictionary<string, ComputedValue> GetValues(string calcConfigurationName)
        {
            return _inner[calcConfigurationName];
        }
    }
}