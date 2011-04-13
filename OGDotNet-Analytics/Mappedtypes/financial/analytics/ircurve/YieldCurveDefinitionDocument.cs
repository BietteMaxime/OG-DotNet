//-----------------------------------------------------------------------
// <copyright file="YieldCurveDefinitionDocument.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.financial.analytics.ircurve
{
    public class YieldCurveDefinitionDocument
    {
        public YieldCurveDefinition Definition { get; set; }

        public UniqueIdentifier UniqueId { get; set; }
    }
}