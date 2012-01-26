//-----------------------------------------------------------------------
// <copyright file="RemotePortfolioMaster.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Master.Portfolio;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    public class RemotePortfolioMaster
    {
        //TODO: this 
        private readonly RestTarget _restTarget;
        private readonly RestTarget _portfoliosTarget;
        private readonly string _activeMQSpec;
        private readonly OpenGammaFudgeContext _fudgeContext;

        public RemotePortfolioMaster(RestTarget restTarget, string activeMQSpec, OpenGammaFudgeContext fudgeContext)
        {
            _restTarget = restTarget;
            _portfoliosTarget = _restTarget.Resolve("portfolios");
            _fudgeContext = fudgeContext;
            _activeMQSpec = activeMQSpec;
        }

        public PortfolioSearchResult Search(PortfolioSearchRequest request)
        {
            string bean = _portfoliosTarget.EncodeBean(request);
            return _portfoliosTarget.Resolve(".", Tuple.Create("msg", bean)).Get<PortfolioSearchResult>();
        }

        public PortfolioHistoryResult GetHistory(PortfolioHistoryRequest request)
        {
            string bean = _portfoliosTarget.EncodeBean(request);
            return _portfoliosTarget.Resolve(request.ObjectId.ToString()).Resolve("versions", Tuple.Create("msg", bean)).Get<PortfolioHistoryResult>();
        }

        public PortfolioDocument Get(UniqueId uniqueId)
        {
            ArgumentChecker.NotNull(uniqueId, "uniqueId");
            var resp = _portfoliosTarget.Resolve(uniqueId.ToString()).Get<PortfolioDocument>();
            if (resp == null || resp.UniqueId == null || resp.Portfolio == null)
            {
                throw new ArgumentException("Not found", "uniqueId");
            }
            return resp;
        }

        public RemoteChangeManger ChangeManager
        {
            get
            {
                return new RemoteChangeManger(_portfoliosTarget.Resolve("changeManager"), _activeMQSpec, _fudgeContext);
            }
        }
    }
}
