// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewCycleExecutionOptions.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using Fudge.Types;

using OpenGamma.Engine.MarketData.Spec;
using OpenGamma.Util;

namespace OpenGamma.Engine.View.Execution
{
    public class ViewCycleExecutionOptions
    {
        private readonly DateTimeOffset _valuationTime;
        private readonly IList<MarketDataSpecification> _marketDataSpecifications;

        public ViewCycleExecutionOptions(DateTimeOffset valuationTime, IList<MarketDataSpecification> marketDataSpecifications)
        {
            ArgumentChecker.NotNull(marketDataSpecifications, "marketDataSpecifications");
            _valuationTime = valuationTime;
            _marketDataSpecifications = marketDataSpecifications;
        }

        public DateTimeOffset ValuationTime
        {
            get { return _valuationTime; }
        }

        public IList<MarketDataSpecification> MarketDataSpecifications
        {
            get { return _marketDataSpecifications; }
        }

        public static ViewCycleExecutionOptions FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var specs = ffc.GetAllByName("marketDataSpecification").Select(deserializer.FromField<MarketDataSpecification>).ToList();
            var valuationValue = ffc.GetValue("valuation");
            var valuation = (FudgeDateTime) (valuationValue == IndicatorType.Instance ? null : valuationValue);
            return new ViewCycleExecutionOptions(valuation.ToDateTimeOffsetWithDefault(), specs);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            if (_valuationTime != default(DateTimeOffset))
            {
                a.Add("valuation", _valuationTime);
            }
            s.WriteAllInline(a, "marketDataSpecification", _marketDataSpecifications);
        }
    }
}