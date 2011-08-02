//-----------------------------------------------------------------------
// <copyright file="Instant.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using OGDotNet.Mappedtypes.Util.Timeseries.Fast;

namespace OGDotNet.Mappedtypes.JavaX.Time
{
    public class Instant
    {
        private readonly long _epochSeconds;
        private readonly long _nanoOfSecond;

        public Instant(long epochSeconds, long nanoOfSecond)
        {
            _epochSeconds = epochSeconds;
            _nanoOfSecond = nanoOfSecond;
        }

        public DateTimeOffset ToDateTimeOffset()
        {
            return new DateTimeOffset(DateTimeNumericEncoding.Epoch) + TimeSpan.FromSeconds(_epochSeconds) + TimeSpan.FromTicks(_nanoOfSecond / 100);
        }

        public long EpochSeconds
        {
            get { return _epochSeconds; }
        }

        public long NanoOfSecond
        {
            get { return _nanoOfSecond; }
        }
    }
}
