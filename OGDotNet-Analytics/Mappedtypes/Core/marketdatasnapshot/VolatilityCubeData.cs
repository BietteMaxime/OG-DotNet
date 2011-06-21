//-----------------------------------------------------------------------
// <copyright file="VolatilityCubeData.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.Core.marketdatasnapshot
{
    public class VolatilityCubeData
    {
        private readonly Dictionary<VolatilityPoint, double> _dataPoints;
        private readonly SnapshotDataBundle _otherData;

        public VolatilityCubeData(Dictionary<VolatilityPoint, double> dataPoints, SnapshotDataBundle otherData)
        {
            _dataPoints = dataPoints;
            _otherData = otherData;
        }

        public Dictionary<VolatilityPoint, double> DataPoints
        {
            get { return _dataPoints; }
        }

        public SnapshotDataBundle OtherData
        {
            get { return _otherData; }
        }

        public static VolatilityCubeData FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var dataPoints = MapBuilder.FromFudgeMsg(ffc.GetMessage("dataPoints"), deserializer.FromField<VolatilityPoint>, f => (double)f.Value);
            var otherValues = deserializer.FromField<SnapshotDataBundle>(ffc.GetByName("otherData"));
            return new VolatilityCubeData(dataPoints, otherValues);
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