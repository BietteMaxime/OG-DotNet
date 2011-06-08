//-----------------------------------------------------------------------
// <copyright file="FixedIncomeStripWithSecurity.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Core.Security;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Util.Time;

namespace OGDotNet.Mappedtypes.financial.analytics.ircurve
{
    public class FixedIncomeStripWithSecurity
    {
        private readonly StripInstrumentType _instrumentType;
        private readonly Tenor _tenor;
        private readonly Tenor _resolvedTenor;
        private readonly int _nthFutureFromTenor;

        private readonly DateTimeOffset _maturity;
        private readonly Identifier _securityIdentifier;
        private readonly ISecurity _security;

        private FixedIncomeStripWithSecurity(StripInstrumentType instrumentType, Tenor tenor, Tenor resolvedTenor, int nthFutureFromTenor, DateTimeOffset maturity, Identifier securityIdentifier, ISecurity security)
        {
            _instrumentType = instrumentType;
            _tenor = tenor;
            _resolvedTenor = resolvedTenor;
            _nthFutureFromTenor = nthFutureFromTenor;
            _maturity = maturity;
            _securityIdentifier = securityIdentifier;
            _security = security;
        }

        public StripInstrumentType InstrumentType
        {
            get { return _instrumentType; }
        }

        public Tenor Tenor
        {
            get { return _tenor; }
        }

        public Tenor ResolvedTenor
        {
            get { return _resolvedTenor; }
        }

        public int NthFutureFromTenor
        {
            get { return _nthFutureFromTenor; }
        }

        public DateTimeOffset Maturity
        {
            get { return _maturity; }
        }

        public Identifier SecurityIdentifier
        {
            get { return _securityIdentifier; }
        }

        public ISecurity Security
        {
            get { return _security; }
        }

        public static FixedIncomeStripWithSecurity FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var ret =
                new FixedIncomeStripWithSecurity(
                    EnumBuilder<StripInstrumentType>.Parse((string)ffc.GetByName("type").Value),
                    new Tenor(ffc.GetString("tenor")),
                    new Tenor(ffc.GetString("resolvedTenor")),
                    ffc.GetInt("numFutures").GetValueOrDefault(-1),
                    GetDateTime(ffc.GetByName("maturity")),
                    Identifier.Parse(ffc.GetString("identifier")),
                    deserializer.FromField<ISecurity>(ffc.GetByName("security"))
                    );

            if ((ret._instrumentType == StripInstrumentType.Future) != ret._nthFutureFromTenor >= 0)
            {
                throw new ArgumentException("Mismatched future options");
            }
            return ret;
        }

        private static DateTimeOffset GetDateTime(IFudgeField zonedDateTimeField)
        {
            var inner = (IFudgeFieldContainer)zonedDateTimeField.Value;
            string zone = inner.GetString("zone"); // TODO this
            string odt = inner.GetString("datetime");
            DateTimeOffset dateTimeOffset = DateTimeOffset.Parse(odt);
            if (zone != "UTC")
                throw new NotImplementedException();
            return dateTimeOffset;
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}