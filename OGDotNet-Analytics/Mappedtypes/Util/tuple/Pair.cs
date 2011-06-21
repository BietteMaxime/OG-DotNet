//-----------------------------------------------------------------------
// <copyright file="Pair.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using Fudge.Types;

namespace OGDotNet.Mappedtypes.Util.tuple
{
    public static class Pair
    {
        public static Pair<TFirst, TSecond> Create<TFirst, TSecond>(TFirst first, TSecond second) where TFirst : class where TSecond : class
        {
            return new Pair<TFirst, TSecond>(first, second);
        }
    }
    public class Pair<TFirst, TSecond> : IEquatable<Pair<TFirst, TSecond>>
        where TFirst : class
        where TSecond : class
    {
        private readonly TFirst _first;
        private readonly TSecond _second;

        public Pair(TFirst first, TSecond second)
        {
            _first = first;
            _second = second;
        }

        public TFirst First
        {
            get { return _first; }
        }

        public TSecond Second
        {
            get { return _second; }
        }

        public static Pair<TFirst, TSecond> FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var first = FromField<TFirst>(deserializer, ffc, "first");
            var second = FromField<TSecond>(deserializer, ffc, "second");
            return new Pair<TFirst, TSecond>(first, second);
        }

        private static T FromField<T>(IFudgeDeserializer deserializer, IFudgeFieldContainer ffc, string fieldName) where T : class
        {
            var field = ffc.GetByName(fieldName);
            if (field.Type != FudgeMsgFieldType.Instance)
            {
                return (T)field.Value;
            }
            return deserializer.FromField<T>(field);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }

        public bool Equals(Pair<TFirst, TSecond> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._first, _first) && Equals(other._second, _second);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Pair<TFirst, TSecond>)) return false;
            return Equals((Pair<TFirst, TSecond>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_first != null ? _first.GetHashCode() : 0) * 397) ^ (_second != null ? _second.GetHashCode() : 0);
            }
        }
    }
}
