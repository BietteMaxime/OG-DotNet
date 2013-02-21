// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Instant.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge.Types;

using OpenGamma.Util.TimeSeries.Fast;

namespace OpenGamma.Time
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

        public Instant(FudgeDateTime versionInstant)
            : this((long)(versionInstant.ToDateTime() - new DateTimeOffset(DateTimeNumericEncoding.Epoch)).TotalSeconds, versionInstant.Nanoseconds)
        {
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
