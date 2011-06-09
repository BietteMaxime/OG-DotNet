//-----------------------------------------------------------------------
// <copyright file="VolatilityCubeData.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.financial.analytics.Volatility.cube;

namespace OGDotNet.Mappedtypes.core.marketdatasnapshot
{
    public class VolatilityCubeData
    {
        private readonly Dictionary<VolatilityPoint, double> _dataPoints;

        public VolatilityCubeData(Dictionary<VolatilityPoint, double> dataPoints)
        {
            _dataPoints = dataPoints;
        }

        public Dictionary<VolatilityPoint, double> DataPoints
        {
            get { return _dataPoints; }
        }

        public static VolatilityCubeData FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var dataPoints = MapBuilder.FromFudgeMsg(ffc.GetMessage("dataPoints"), deserializer.FromField<VolatilityPoint>, f => (double)f.Value);
            return new VolatilityCubeData(dataPoints);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}