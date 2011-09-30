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
        private readonly string _activeMQSpec;
        private readonly OpenGammaFudgeContext _fudgeContext;

        public RemotePortfolioMaster(RestTarget restTarget, string activeMQSpec, OpenGammaFudgeContext fudgeContext)
        {
            _restTarget = restTarget;
            _fudgeContext = fudgeContext;
            _activeMQSpec = activeMQSpec;
        }

        public PortfolioSearchResult Search(PortfolioSearchRequest request)
        {
            string bean = _restTarget.EncodeBean(request);
            return _restTarget.Resolve(".", Tuple.Create("msg", bean)).Get<PortfolioSearchResult>();
        }

        public PortfolioHistoryResult GetHistory(PortfolioHistoryRequest request)
        {
            string bean = _restTarget.EncodeBean(request);
            return _restTarget.Resolve(request.ObjectId.ToString()).Resolve("versions", Tuple.Create("msg", bean)).Get<PortfolioHistoryResult>();
        }

        public RemoteChangeManger ChangeManager
        {
            get
            {
                return new RemoteChangeManger(_restTarget.Resolve("changeManager"), _activeMQSpec, _fudgeContext);
            }
        }
    }
}
