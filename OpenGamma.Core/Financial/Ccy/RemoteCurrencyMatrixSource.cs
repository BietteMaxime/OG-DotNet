// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteCurrencyMatrixSource.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Model;

namespace OpenGamma.Financial.Ccy
{
    public class RemoteCurrencyMatrixSource
    {
        private readonly RestTarget _rest;

        public RemoteCurrencyMatrixSource(RestTarget rest)
        {
            _rest = rest;
        }

        public ICurrencyMatrix GetCurrencyMatrix(string name)
        {
            return _rest.Resolve("currencyMatrices").Resolve(name).Get<ICurrencyMatrix>();
        }
    }
}