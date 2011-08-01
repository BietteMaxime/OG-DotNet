//-----------------------------------------------------------------------
// <copyright file="FuturePriceCurveData.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
namespace OGDotNet.Mappedtypes.financial.analytics.Volatility.Surface
{
    /// <summary>
    /// TODO: the rest of this
    /// NOTE DOTNET-14 makes target hard
    /// </summary>
    public class FuturePriceCurveData
    {
        private readonly string _definitionName;
        private readonly string _specificationName;

        public FuturePriceCurveData(string definitionName, string specificationName)
        {
            _definitionName = definitionName;
            _specificationName = specificationName;
        }

        public string DefinitionName
        {
            get { return _definitionName; }
        }

        public string SpecificationName
        {
            get { return _specificationName; }
        }
    }
}
