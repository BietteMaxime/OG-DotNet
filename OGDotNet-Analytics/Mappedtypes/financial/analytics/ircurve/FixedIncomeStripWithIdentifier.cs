//-----------------------------------------------------------------------
// <copyright file="FixedIncomeStripWithIdentifier.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Financial.Analytics.IRCurve
{
    public class FixedIncomeStripWithIdentifier 
    {
        private readonly FixedIncomeStrip _strip;
        private readonly ExternalId _security;

        public FixedIncomeStripWithIdentifier(ExternalId security, FixedIncomeStrip strip)
        {
            _security = security;
            _strip = strip;
        }

        public FixedIncomeStrip Strip
        {
            get { return _strip; }
        }

        public ExternalId Security
        {
            get { return _security; }
        }

        public static FixedIncomeStripWithIdentifier FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new FixedIncomeStripWithIdentifier(ExternalId.Parse(ffc.GetString("identifier")), deserializer.FromField<FixedIncomeStrip>(ffc.GetByName("strip")));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            s.WriteInline(a, "identifier", _security);
            s.WriteInline(a, "strip", _strip);
        }
    }
}