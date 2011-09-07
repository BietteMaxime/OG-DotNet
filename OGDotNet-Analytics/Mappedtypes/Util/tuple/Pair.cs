//-----------------------------------------------------------------------
// <copyright file="Pair.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Linq;
using System.Reflection;
using Fudge;
using Fudge.Serialization;
using Fudge.Types;
using OGDotNet.Builders;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.Util.Tuple
{
    public interface IPair<out TFirst, out TSecond>
    {
        TFirst First { get; }
        TSecond Second { get; }
    }

    public abstract class Pair
    {
        public static Pair<TFirst, TSecond> Create<TFirst, TSecond>(TFirst first, TSecond second)
        {
            return new Pair<TFirst, TSecond>(first, second);
        }

        public static Pair FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var first = FromField(deserializer, ffc, "first");
            var second = FromField(deserializer, ffc, "second");
            return Build(first, second);
        }

        static readonly MethodInfo GenericBuildMethod = GenericUtils.GetGenericMethod(typeof(Pair), "Build");

        private static Pair Build(object first, object second)
        {
            return (Pair)GenericUtils.Call(GenericBuildMethod, new[] { GetType(first.GetType()), GetType(second.GetType()) }, first, second);
        }

        private static Type GetType(Type real)
        {
            return real == typeof(sbyte) ? typeof(long) : real;
        }

        public static Pair<TFirst, TSecond> Build<TFirst, TSecond>(TFirst first, TSecond second)
        {
            return new Pair<TFirst, TSecond>(first, second);
        }

        private static object FromField(IFudgeDeserializer deserializer, IFudgeFieldContainer ffc, string fieldName)
        {
            var valueField = ffc.GetByName(fieldName);
            return FromField(deserializer, valueField);
        }

        public static object FromField(IFudgeDeserializer deserializer, IFudgeField valueField)
        {
            if (valueField.Type != FudgeMsgFieldType.Instance)
            {
                return valueField.Value;
            }

            var fudgeFieldContainer = (IFudgeFieldContainer)valueField.Value;

            foreach (var typeField in fudgeFieldContainer.GetAllByOrdinal(0))
            {
                switch (typeField.Value as string)
                {
                    case "java.lang.Number":
                        return fudgeFieldContainer.GetValue("value");
                    default:
                        continue;
                }
            }
            return deserializer.FromField(valueField, null);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }

    public class Pair<TFirst, TSecond> : Pair, IEquatable<Pair<TFirst, TSecond>>, IPair<TFirst, TSecond>, IComparable, IComparable<Pair<TFirst, TSecond>>
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

        public static new Pair<TFirst, TSecond> FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var first = FromField<TFirst>(deserializer, ffc, "first");
            var second = FromField<TSecond>(deserializer, ffc, "second");
            return new Pair<TFirst, TSecond>(first, second);
        }

        private static T FromField<T>(IFudgeDeserializer deserializer, IFudgeFieldContainer ffc, string fieldName)
        {
            var field = ffc.GetByName(fieldName);
            object value = FromField(deserializer, field);
            if (typeof(T) == typeof(long))
            {
                return (T)(object)Convert.ToInt64(value);
            }
            else if (typeof(T) == typeof(int))
            {
                return (T)(object)Convert.ToInt32(value);
            }
            else
            {
                return (T)value;
            }
        }

        public new void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            var fudgeSerializer = new FudgeSerializer(s.Context);
            WriteInline(a, s, "first", First, fudgeSerializer);
            WriteInline(a, s, "second", Second, fudgeSerializer);
        }

        private static void WriteInline(IAppendingFudgeFieldContainer a, IFudgeSerializer s, string name, object o, FudgeSerializer fudgeSerializer)
        {
            var oType = o.GetType();
            if (s.Context.TypeDictionary.GetByCSharpType(oType) != null)
            {
                a.Add(name, o);
            }
            else
            {
                var serializeToMsg = fudgeSerializer.SerializeToMsg(o);
                s.WriteTypeHeader(serializeToMsg, oType);
                a.Add(name, serializeToMsg);
            }
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
            return Equals((Pair<TFirst, TSecond>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_first != null ? _first.GetHashCode() : 0) * 397) ^ (_second != null ? _second.GetHashCode() : 0);
            }
        }

        public int CompareTo(object obj)
        {
            return CompareTo((Pair<TFirst, TSecond>)obj);
        }

        public int CompareTo(Pair<TFirst, TSecond> other)
        {
            return ((IComparable)AsTuple()).CompareTo(((IComparable)other.AsTuple()));
        }

        public override string ToString()
        {
            return AsTuple().ToString();
        }

        private Tuple<TFirst, TSecond> AsTuple()
        {
            return new Tuple<TFirst, TSecond>(First, Second);
        }
    }
}
