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

namespace OGDotNet.Mappedtypes.engine.View.Execution
{
    public class ViewCycleExecutionOptions
    {
        private readonly DateTimeOffset _valuationTime;
        private readonly DateTimeOffset _inputDataTime;

        public ViewCycleExecutionOptions(DateTimeOffset valuationTime, DateTimeOffset inputDataTime)
        {
            _valuationTime = valuationTime;
            _inputDataTime = inputDataTime;
        }

        public DateTimeOffset ValuationTime
        {
            get { return _valuationTime; }
        }

        public DateTimeOffset InputDataTime
        {
            get { return _inputDataTime; }
        }

        public static ViewCycleExecutionOptions FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new ViewCycleExecutionOptions(ffc.GetValue<DateTimeOffset>("valuation"), ffc.GetValue<DateTimeOffset>("inputData"));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            a.Add("valuation", _valuationTime);
            a.Add("inputData", _inputDataTime);
        }
    }
}