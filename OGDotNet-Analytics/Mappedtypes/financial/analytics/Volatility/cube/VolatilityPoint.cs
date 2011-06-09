//-----------------------------------------------------------------------
// <copyright file="VolatilityPoint.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Util.Time;

namespace OGDotNet.Mappedtypes.financial.analytics.Volatility.cube
{
    public class VolatilityPoint : IEquatable<VolatilityPoint>
    {
        private readonly Tenor _swapTenor;
        private readonly Tenor _optionExpiry;
        private readonly double _relativeStrike;

        public VolatilityPoint(Tenor swapTenor, Tenor optionExpiry, double relativeStrike)
        {
            _swapTenor = swapTenor;
            _optionExpiry = optionExpiry;
            _relativeStrike = relativeStrike;
        }

        public Tenor SwapTenor
        {
            get { return _swapTenor; }
        }

        public Tenor OptionExpiry
        {
            get { return _optionExpiry; }
        }

        public double RelativeStrike
        {
            get { return _relativeStrike; }
        }

        public bool Equals(VolatilityPoint other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._swapTenor, _swapTenor) && Equals(other._optionExpiry, _optionExpiry) && other._relativeStrike.Equals(_relativeStrike);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(VolatilityPoint)) return false;
            return Equals((VolatilityPoint)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = _swapTenor.GetHashCode();
                result = (result * 397) ^ _optionExpiry.GetHashCode();
                result = (result * 397) ^ _relativeStrike.GetHashCode();
                return result;
            }
        }

        public static VolatilityPoint FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new VolatilityPoint(
                deserializer.FromField<Tenor>(ffc.GetByName("swapTenor")),
                deserializer.FromField<Tenor>(ffc.GetByName("optionExpiry")),
                ffc.GetDouble("relativeStrike").Value
                );
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            s.WriteInline(a, "swapTenor", _swapTenor);
            s.WriteInline(a, "optionExpiry", _optionExpiry);

            a.Add("relativeStrike", _relativeStrike);
        }
    }
}