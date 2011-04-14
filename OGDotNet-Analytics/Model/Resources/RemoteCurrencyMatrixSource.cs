//-----------------------------------------------------------------------
// <copyright file="RemoteCurrencyMatrixSource.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.financial.currency;

namespace OGDotNet.Model.Resources
{
    public class RemoteCurrencyMatrixSource
    {
        private readonly RestTarget _rest;

        public RemoteCurrencyMatrixSource(RestTarget rest)
        {
            _rest = rest;
        }

        public CurrencyMatrix GetCurrencyMatrix(string name)
        {
            return _rest.Resolve(name).Get<CurrencyMatrix>("matrix");
        }
    }
}