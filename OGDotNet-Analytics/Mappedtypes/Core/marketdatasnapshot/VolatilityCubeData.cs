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
using OGDotNet.Mappedtypes.Util.Time;
using OGDotNet.Mappedtypes.Util.tuple;

namespace OGDotNet.Mappedtypes.Core.marketdatasnapshot
{
    public class VolatilityCubeData
    {
        private readonly Dictionary<VolatilityPoint, double> _dataPoints;
        private readonly SnapshotDataBundle _otherData;
        private readonly Dictionary<Pair<Tenor, Tenor>, double> _strikes;

        public VolatilityCubeData(Dictionary<VolatilityPoint, double> dataPoints, SnapshotDataBundle otherData, Dictionary<Pair<Tenor, Tenor>, double> strikes)
        {
            _dataPoints = dataPoints;
            _otherData = otherData;
            _strikes = strikes;
        }

        public Dictionary<VolatilityPoint, double> DataPoints
        {
            get { return _dataPoints; }
        }

        public SnapshotDataBundle OtherData
        {
            get { return _otherData; }
        }

        public Dictionary<Pair<Tenor, Tenor>, double> Strikes
        {
            get { return _strikes; }
        }

        public static VolatilityCubeData FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var dataPoints = MapBuilder.FromFudgeMsg(ffc.GetMessage("dataPoints"), deserializer.FromField<VolatilityPoint>, f => (double)f.Value);
            var otherValues = deserializer.FromField<SnapshotDataBundle>(ffc.GetByName("otherData"));

            Dictionary<Pair<Tenor, Tenor>, double> strikes = null;

            var strikesField = ffc.GetMessage("strikes");
            if (strikesField != null)
            {
                strikes = strikesField.Select(deserializer.FromField<StrikeEntry>).ToDictionary(s => new Pair<Tenor, Tenor>(new Tenor(s.SwapTenor), new Tenor(s.OptionExpiry)), s => s.Strike);
            }
            return new VolatilityCubeData(dataPoints, otherValues, strikes);
        }

        private class StrikeEntry
        {
            public string SwapTenor { get; set; }
            public string OptionExpiry { get; set; }
            public double Strike { get; set; }
        }
        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("[VolatilityCubeData {0} points]", _dataPoints.Count);
        }
    }
}