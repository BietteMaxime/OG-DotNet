//-----------------------------------------------------------------------
// <copyright file="VolatilitySurfaceKey.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Financial.Analytics.Volatility.Cube
{
    public class VolatilitySurfaceKey : IComparable<VolatilitySurfaceKey>
    {
        private readonly UniqueIdentifier _target;
        private readonly string _name;
        private readonly string _instrumentType;

        public VolatilitySurfaceKey(UniqueIdentifier target, string name, string instrumentType)
        {
            _target = target;
            _name = name;
            _instrumentType = instrumentType;
        }

        public UniqueIdentifier Target
        {
            get { return _target; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string InstrumentType
        {
            get { return _instrumentType; }
        }

        public override string ToString()
        {
            return string.Format("[VolatilitySurfaceKey {0} {1} {2}]", _target.Value, _name, _instrumentType);
        }
        public static VolatilitySurfaceKey FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new VolatilitySurfaceKey(UniqueIdentifier.Parse(ffc.GetString("target")), ffc.GetString("name"), ffc.GetString("instrumentType"));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            a.Add("target", Target.ToString());
            a.Add("name", Name);
            a.Add("instrumentType", InstrumentType);
        }

        public int CompareTo(VolatilitySurfaceKey other)
        {
            int ret = _target.CompareTo(other._target);
            if (ret != 0)
                return ret;
            ret = _name.CompareTo(other._name);
            if (ret != 0)
                return ret;
            return _instrumentType.CompareTo(other._instrumentType);
        }

        public bool Equals(VolatilitySurfaceKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._target, _target) && Equals(other._name, _name) && Equals(other._instrumentType, _instrumentType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(VolatilitySurfaceKey)) return false;
            return Equals((VolatilitySurfaceKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_target != null ? _target.GetHashCode() : 0) * 397) ^ (_name != null ? _name.GetHashCode() : 0);
            }
        }
    }
}