﻿using System;
using System.Collections.Generic;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using System.Linq;
using OGDotNet.Mappedtypes.master.marketdatasnapshot;

namespace OGDotNet.Model.Context.MarketDataSnapshot.Warnings
{
    public abstract class Warning
    {
        private readonly string _message;

        internal Warning(string message)
        {
            _message = message;
        }

        public string Message
        {
            get { return _message; }
        }
    }

    public class OverriddenValueDisappearingWarning : Warning
    {//TODO does the user need to know scope?
        private OverriddenValueDisappearingWarning(MarketDataValueSpecification spec, string valueName)
            : base(String.Format("Value {0} on {1} will not be present in the new snapshot, overrides will be lost", valueName, spec))
        {
        }

        public static IEnumerable<Warning> Of(MarketDataValueSpecification spec, string k, ValueSnapshot v)
        {
            return v.OverrideValue.HasValue ? new Warning[] { new OverriddenValueDisappearingWarning(spec, k) } : new Warning[] { };
        }
    }
    public class OverriddenSecurityDisappearingWarning : Warning
    {//TODO does the user need to know scope?
        private OverriddenSecurityDisappearingWarning(MarketDataValueSpecification spec, IEnumerable<KeyValuePair<string, ValueSnapshot>> overrides)
            : base(string.Format("{0} will not be present in the new snapshot, overrides on ({1}) will be lost", spec, string.Join(",", overrides.Select(o => o.Key))))
        {

        }

        public static IEnumerable<Warning> Of(MarketDataValueSpecification spec, IEnumerable<KeyValuePair<string, ValueSnapshot>> values)
        {
            var overrides = values.Where(v => v.Value.OverrideValue.HasValue);
            return overrides.Any(v => v.Value.OverrideValue.HasValue) ? new Warning[] { new OverriddenSecurityDisappearingWarning(spec, overrides) } : new Warning[] { };
        }
    }
    public class OverriddenYieldCurveDisappearingWarning : Warning
    {
        private OverriddenYieldCurveDisappearingWarning(YieldCurveKey key)
            : base(string.Format("Yield Curve {0} {1} will not be present in the new snapshot, overrides will be lost", key.Currency, key.Name))
        {

        }

        public static IEnumerable<Warning> Of(YieldCurveKey key, ManageableYieldCurveSnapshot value)
        {
            return value.HaveOverrides() ? new Warning[] { new OverriddenYieldCurveDisappearingWarning(key) } : new Warning[] { };
        }
    }
}