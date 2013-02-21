// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComputationCacheQuery.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

using Fudge.Serialization;

using OpenGamma.Engine.Value;
using OpenGamma.Fudge;

namespace OpenGamma.Engine.View.Calc
{
    [FudgeSurrogate(typeof(ComputationCacheQueryBuilder))]
    public class ComputationCacheQuery
    {
        private readonly string _calculationConfigurationName;
        private readonly IEnumerable<ValueSpecification> _valueSpecifications;

        public ComputationCacheQuery(string calculationConfigurationName, params ValueSpecification[] valueSpecifications) : this (calculationConfigurationName, valueSpecifications.ToList())
        {
        }

        public ComputationCacheQuery(string calculationConfigurationName, IEnumerable<ValueSpecification> valueSpecifications)
        {
            _calculationConfigurationName = calculationConfigurationName;
            _valueSpecifications = valueSpecifications.ToArray();
        }

        public string CalculationConfigurationName
        {
            get { return _calculationConfigurationName; }
        }

        public IEnumerable<ValueSpecification> ValueSpecifications
        {
            get { return _valueSpecifications; }
        }
    }
}
