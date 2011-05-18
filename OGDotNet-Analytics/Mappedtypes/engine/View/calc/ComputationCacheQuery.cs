//-----------------------------------------------------------------------
// <copyright file="ComputationCacheQuery.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
using OGDotNet.Mappedtypes.engine.Value;
using OGDotNet.Model;

namespace OGDotNet.Mappedtypes.engine.View.calc
{
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

        public static ComputationCacheQuery FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            throw new NotImplementedException();
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            a.Add("calculationConfigurationName", _calculationConfigurationName);
            var fudgeMsg = new FudgeMsg();
            var s2 = ((OpenGammaFudgeContext) s.Context).GetSerializer();
            foreach (var valueSpecification in _valueSpecifications)
            {
                fudgeMsg.Add(null, null, s2.SerializeToMsg(valueSpecification));
            }
            a.Add("valueSpecifications", fudgeMsg);
        }
    }
}
