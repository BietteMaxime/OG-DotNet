//-----------------------------------------------------------------------
// <copyright file="NumberDeltaComparer.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Mappedtypes.engine.view
{
    public class NumberDeltaComparer : IDeltaComparer<double>, IEquatable<IDeltaComparer<double>>
    {
        private readonly int _decimalPlaces;

        public NumberDeltaComparer(int decimalPlaces)
        {
            _decimalPlaces = decimalPlaces;
        }

        public bool IsDelta(double previousValue, double newValue)
        {
            throw new NotImplementedException();
        }

        public static NumberDeltaComparer FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new NumberDeltaComparer(ffc.GetInt("decimalPlaces").Value);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            a.Add("decimalPlaces", _decimalPlaces);
        }

        public bool Equals(IDeltaComparer<double> other)
        {
            return other is NumberDeltaComparer && Equals((NumberDeltaComparer) other);
        }

        public bool Equals(NumberDeltaComparer other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other._decimalPlaces == _decimalPlaces;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (NumberDeltaComparer)) return false;
            return Equals((NumberDeltaComparer) obj);
        }

        public override int GetHashCode()
        {
            return _decimalPlaces;
        }
    }
}