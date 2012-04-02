//-----------------------------------------------------------------------
// <copyright file="UnorderedCurrencyPair.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Util.Money
{
    [FudgeSurrogate(typeof(UnorderedCurrencyPairBuilder))]
    public class UnorderedCurrencyPair : IUniqueIdentifiable
    {
        public const string ObjectScheme = "UnorderedCurrencyPair";

        private readonly Currency _ccy1;
        private readonly Currency _ccy2;
        private readonly string _idValue;

        public UnorderedCurrencyPair(Currency ccy1, Currency ccy2)
        {
            if (string.CompareOrdinal(ccy1.ISOCode, ccy2.ISOCode) < 0)
            {
                _ccy1 = ccy1;
                _ccy2 = ccy2;
            } else
            {
                _ccy1 = ccy2;
                _ccy2 = ccy1;
            }
            _idValue = _ccy1.ISOCode + _ccy2.ISOCode;
        }

        public Currency FirstCurrecy
        {
            get { return _ccy1; }
        }

        public Currency SecondCurrency
        {
            get { return _ccy2; }
        }

        public UniqueId UniqueId
        {
            get { return UniqueId.Create(ObjectScheme, _idValue); }
        }
    }
}
