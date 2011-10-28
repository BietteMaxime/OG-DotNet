//-----------------------------------------------------------------------
// <copyright file="MarketDataValueSpecification.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Core.MarketDataSnapshot
{
    public class MarketDataValueSpecification : IEquatable<MarketDataValueSpecification>
    {
        private readonly MarketDataValueType _type;
        private readonly UniqueId _uniqueId;

        public MarketDataValueSpecification(MarketDataValueType type, UniqueId uniqueId)
        {
            _type = type;
            _uniqueId = uniqueId;
        }

        public MarketDataValueType Type
        {
            get { return _type; }
        }

        public UniqueId UniqueId
        {
            get { return _uniqueId; }
        }

        public bool Equals(MarketDataValueSpecification other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._type, _type) && Equals(other._uniqueId, _uniqueId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(MarketDataValueSpecification)) return false;
            return Equals((MarketDataValueSpecification)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_type.GetHashCode() * 397) ^ (_uniqueId != null ? _uniqueId.GetHashCode() : 0);
            }
        }

        public static MarketDataValueSpecification FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var marketDataValueType = EnumBuilder<MarketDataValueType>.Parse(ffc.GetString("type"));
            return new MarketDataValueSpecification(
                marketDataValueType,
                UniqueId.Parse(ffc.GetString("uniqueId"))
                );
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            a.Add("type", EnumBuilder<MarketDataValueType>.GetJavaName(_type));
            a.Add("uniqueId", _uniqueId.ToString());
        }

        public override string ToString()
        {
            return string.Format("[{0} {1}]", _type, _uniqueId);
        }
    }
}