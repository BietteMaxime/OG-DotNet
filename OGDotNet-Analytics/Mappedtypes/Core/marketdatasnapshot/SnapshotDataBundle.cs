//-----------------------------------------------------------------------
// <copyright file="SnapshotDataBundle.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.Core.MarketDataSnapshot
{
    public class SnapshotDataBundle
    {
        private readonly Dictionary<UniqueId, double> _dataPoints;

        public SnapshotDataBundle(Dictionary<UniqueId, double> dataPoints)
        {
            ArgumentChecker.NotNull(dataPoints, "dataPoints");
            _dataPoints = dataPoints;
        }

        public Dictionary<UniqueId, double> DataPoints
        {
            get { return _dataPoints; }
        }

        public static SnapshotDataBundle FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var map = MapBuilder.FromFudgeMsg(ffc.GetMessage("dataPoints"), f => UniqueId.Parse((string)f.Value), f => (double)f.Value);
            return new SnapshotDataBundle(map);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}
