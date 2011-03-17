using System;
using System.Collections.Generic;
using System.ComponentModel;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine;

namespace OGDotNet.Mappedtypes.Master.MarketDataSnapshot
{
    public class YieldCurveSnapshot : MarketDataSnapshotScope, INotifyPropertyChanged
    {
        private DateTimeOffset _valuationTime;

        public YieldCurveSnapshot(IDictionary<ComputationTargetSpecification, IDictionary<string, ValueSnapshot>> values, DateTimeOffset valuationTime)
            : base(values)
        {
            _valuationTime = valuationTime;
        }


        public DateTimeOffset ValuationTime
        {
            get { return _valuationTime; }
        }


        public void UpdateFrom(YieldCurveSnapshot yieldCurve)
        {
            base.UpdateFrom(yieldCurve);
            
            _valuationTime = yieldCurve.ValuationTime;
            InvokePropertyChanged(new PropertyChangedEventArgs("ValuationTime"));
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }
    }
}