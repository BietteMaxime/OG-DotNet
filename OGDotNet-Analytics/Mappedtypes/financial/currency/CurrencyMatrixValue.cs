//-----------------------------------------------------------------------
// <copyright file="CurrencyMatrixValue.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.Common;
using OGDotNet.Mappedtypes.engine.value;

namespace OGDotNet.Mappedtypes.financial.currency
{
    public abstract class CurrencyMatrixValue : IEquatable<CurrencyMatrixValue>
    {
        public abstract CurrencyMatrixValue GetReciprocal();

        public static CurrencyMatrixValue Of(Currency currency)
        {
            return new CurrencyMatrixCross(currency);
        }

        public static CurrencyMatrixValue Of(double fixedValue)
        {
            return new CurrencyMatrixFixed(fixedValue);
        }

        public class CurrencyMatrixFixed : CurrencyMatrixValue
        {
            private readonly double _fixedValue;

            public CurrencyMatrixFixed(double fixedValue)
            {
                _fixedValue = fixedValue;
            }

            public double FixedValue
            {
                get { return _fixedValue; }
            }

            public override CurrencyMatrixValue GetReciprocal()
            {
                return new CurrencyMatrixFixed(1 / _fixedValue);
            }

            protected override bool EqualsInner(CurrencyMatrixValue other)
            {
                return ((CurrencyMatrixFixed) other)._fixedValue == _fixedValue; // TODO : this is an odd thing to do, since == on doubles rarely does what you want.  But it's what the Java side does
            }
        }

        public class CurrencyMatrixCross : CurrencyMatrixValue
        {
            private Currency _crossCurrency;

            public CurrencyMatrixCross(Currency crossCurrency)
            {
                _crossCurrency = crossCurrency;
            }

            public Currency CrossCurrency
            {
                get { return _crossCurrency; }
                set { _crossCurrency = value; }
            }

            public override CurrencyMatrixValue GetReciprocal()
            {
                return this;
            }
            protected override bool EqualsInner(CurrencyMatrixValue other)
            {
                return ((CurrencyMatrixCross)other)._crossCurrency == _crossCurrency;
            }
        }

        public class CurrencyMatrixValueRequirement : CurrencyMatrixValue
        {
            private readonly ValueRequirement _valueRequirement;
            private readonly bool _reciprocal;

            private CurrencyMatrixValueRequirement(ValueRequirement valueRequirement, bool reciprocal)
            {
                _valueRequirement = valueRequirement;
                _reciprocal = reciprocal;
            }

            public ValueRequirement ValueRequirement
            {
                get { return _valueRequirement; }
            }

            public bool Reciprocal
            {
                get { return _reciprocal; }
            }

            public override CurrencyMatrixValue GetReciprocal()
            {
                return new CurrencyMatrixValueRequirement(_valueRequirement, !_reciprocal);
            }

            public static CurrencyMatrixValueRequirement FromFudgeMsg(IFudgeFieldContainer message, IFudgeDeserializer deserializer)
            {
                var fudgeSerializer = new FudgeSerializer(deserializer.Context);
                return new CurrencyMatrixValueRequirement(fudgeSerializer.Deserialize<ValueRequirement>((FudgeMsg)message), message.GetBoolean("reciprocal").Value);
            }

            public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
            {
                throw new NotImplementedException();
            }

            protected override bool EqualsInner(CurrencyMatrixValue other)
            {
                var otherReq = (CurrencyMatrixValueRequirement)other;
                return otherReq._valueRequirement == this.ValueRequirement && otherReq._reciprocal == this._reciprocal;
            }
        }

        public bool Equals(CurrencyMatrixValue other)
        {
            if (other == null)
                return false;
            if (other.GetType() != this.GetType())
                return false;
            return EqualsInner(other);
        }

        protected abstract bool EqualsInner(CurrencyMatrixValue other);
    }
}