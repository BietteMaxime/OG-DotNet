//-----------------------------------------------------------------------
// <copyright file="FinancialUser.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using OGDotNet.Model;
using OGDotNet.Model.Context;

namespace OGDotNet.Mappedtypes.Financial.User
{
    public class FinancialUser
    {
        private readonly OpenGammaFudgeContext _fudgeContext;
        private readonly RestTarget _restTarget;
        private readonly string _userName;

        public FinancialUser(OpenGammaFudgeContext fudgeContext, RestTarget restTarget, string userName)
        {
            _fudgeContext = fudgeContext;
            _restTarget = restTarget;
            _userName = userName;
        }

        public FinancialClient CreateClient()
        {
            return GetOrCreateClient(Guid.NewGuid().ToString());
        }

        private FinancialClient GetOrCreateClient(string clientName)
        {
            return new FinancialClient(_restTarget.Resolve("clients", clientName), _fudgeContext);
        }

        public string UserName
        {
            get { return _userName; }
        }
    }
}
