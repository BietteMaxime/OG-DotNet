//-----------------------------------------------------------------------
// <copyright file="RemotePortfolioMaster.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using OGDotNet.Mappedtypes.Master.Portfolio;

namespace OGDotNet.Model.Resources
{
    public class RemotePortfolioMaster
    {
        //TODO: this 
        private readonly RestTarget _restTarget;

        public RemotePortfolioMaster(RestTarget restTarget)
        {
            _restTarget = restTarget;
        }

        public PortfolioSearchResult Search(PortfolioSearchRequest request)
        {
            string bean = _restTarget.EncodeBean(request);
            return _restTarget.Resolve(".", Tuple.Create("msg", bean)).Get<PortfolioSearchResult>();
        }
    }
}
