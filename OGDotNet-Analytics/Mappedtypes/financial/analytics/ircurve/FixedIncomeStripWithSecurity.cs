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

namespace OGDotNet.Mappedtypes.Financial.Analytics.IRCurve
{
    public class FixedIncomeStripWithSecurity
    {
        private readonly FixedIncomeStrip _originalStrip;        
        private readonly Tenor _resolvedTenor;
        private readonly DateTimeOffset _maturity;
        private readonly ExternalId _securityIdentifier;
        private readonly ISecurity _security;

        private FixedIncomeStripWithSecurity(FixedIncomeStrip originalStrip, Tenor resolvedTenor, DateTimeOffset maturity, ExternalId securityIdentifier, ISecurity security)
        {
            _resolvedTenor = resolvedTenor;
            _originalStrip = originalStrip;
            _maturity = maturity;
            _securityIdentifier = securityIdentifier;
            _security = security;
        }

        public FixedIncomeStrip OriginalStrip
        {
            get { return _originalStrip; }
        }

        public Tenor ResolvedTenor
        {
            get { return _resolvedTenor; }
        }

        public DateTimeOffset Maturity
        {
            get { return _maturity; }
        }

        public ExternalId SecurityIdentifier
        {
            get { return _securityIdentifier; }
        }

        public ISecurity Security
        {
            get { return _security; }
        }

        public static FixedIncomeStripWithSecurity FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var originalstrip = deserializer.FromField<FixedIncomeStrip>(ffc.GetByName("strip"));
            var resolvedTenor = new Tenor(ffc.GetString("resolvedTenor"));
            DateTimeOffset maturity = GetDateTime(ffc.GetByName("maturity"));
            ExternalId securityIdentifier = ExternalId.Parse(ffc.GetString("identifier"));
            var security = deserializer.FromField<ISecurity>(ffc.GetByName("security"));
            return new FixedIncomeStripWithSecurity(originalstrip, 
                                                    resolvedTenor,
                                                    maturity,
                                                    securityIdentifier,
                                                    security
                );
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