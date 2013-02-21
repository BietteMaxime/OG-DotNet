// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VersionCorrection.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge.Serialization;

using OpenGamma.Fudge;

namespace OpenGamma.Id
{
    [FudgeSurrogate(typeof(VersionCorrectionBuilder))]
    public class VersionCorrection : IEquatable<VersionCorrection>
    {
        public static VersionCorrection Latest
        {
            get { return new VersionCorrection(default(DateTimeOffset), default(DateTimeOffset)); }
        }

        private readonly DateTimeOffset _versionAsOf;
        private readonly DateTimeOffset _correctedTo;

        public VersionCorrection(DateTimeOffset versionAsOf, DateTimeOffset correctedTo)
        {
            _versionAsOf = versionAsOf;
            _correctedTo = correctedTo;
        }

        public DateTimeOffset VersionAsOf
        {
            get { return _versionAsOf; }
        }

        public string VersionAsOfString
        {
            get
            {
                return VersionAsOf != default(DateTimeOffset) ? VersionAsOf.ToString() : "LATEST";
            }
        }

        public DateTimeOffset CorrectedTo
        {
            get { return _correctedTo; }
        }

        public string CorrectedToString
        {
            get
            {
                return CorrectedTo != default(DateTimeOffset) ? CorrectedTo.ToString() : "LATEST";
            }
        }

        public bool Equals(VersionCorrection other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other._versionAsOf.Equals(_versionAsOf) && other._correctedTo.Equals(_correctedTo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(VersionCorrection)) return false;
            return Equals((VersionCorrection) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_versionAsOf.GetHashCode() * 397) ^ _correctedTo.GetHashCode();
            }
        }

        public override string ToString()
        {
            return string.Format("V{0}.C{1}", ToInstantString(VersionAsOf), ToInstantString(CorrectedTo));
        }

        private string ToInstantString(DateTimeOffset dateTime)
        {
            return dateTime == default(DateTimeOffset) ? "LATEST" : dateTime.ToString();
        }
    }
}
