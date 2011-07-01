//-----------------------------------------------------------------------
// <copyright file="ViewCycleExecutionOptions.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using Fudge.Types;
using OGDotNet.Mappedtypes.engine.marketdata.spec;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.engine.View.Execution
{
    public class ViewCycleExecutionOptions
    {
        private readonly DateTimeOffset _valuationTime;
        private readonly MarketDataSpecification _marketDataSpecification;

        public ViewCycleExecutionOptions(DateTimeOffset valuationTime, MarketDataSpecification marketDataSpecification)
        {
            _valuationTime = valuationTime;
            _marketDataSpecification = marketDataSpecification;
        }

        public DateTimeOffset ValuationTime
        {
            get { return _valuationTime; }
        }

        public MarketDataSpecification MarketDataSpecification
        {
            get { return _marketDataSpecification; }
        }

        public static ViewCycleExecutionOptions FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            IFudgeField mdsField = ffc.GetByName("marketDataSpecification");
            var marketDataSpecification = mdsField  == null ? null : deserializer.FromField<MarketDataSpecification>(mdsField);
            return new ViewCycleExecutionOptions(((FudgeDateTime) ffc.GetValue("valuation")).ToDateTimeOffsetWithDefault(), marketDataSpecification);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            if (_valuationTime != default(DateTimeOffset))
            {
                a.Add("valuation", _valuationTime);
            }
            if (_marketDataSpecification != null)
            {
                s.WriteInline(a, "marketDataSpecification", _marketDataSpecification);
            }
        }
    }
}