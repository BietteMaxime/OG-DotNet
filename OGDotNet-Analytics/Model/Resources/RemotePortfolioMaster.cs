//-----------------------------------------------------------------------
// <copyright file="RemotePortfolioMaster.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge.Serialization.Reflection;
using Fudge.Types;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Master.Portfolio;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    public class RemotePortfolioMaster
    {
        private readonly RestTarget _restTarget;
        private readonly OpenGammaFudgeContext _fudgeContext;

        public RemotePortfolioMaster(RestTarget restTarget, OpenGammaFudgeContext fudgeContext)
        {
            _restTarget = restTarget;
            _fudgeContext = fudgeContext;
        }

        public PortfolioSearchResult Search(PortfolioSearchRequest request)
        {
            return _restTarget.Resolve("portfolioSearches").Post<PortfolioSearchResult>(request);
        }

        public PortfolioHistoryResult GetHistory(PortfolioHistoryRequest request)
        {
            //request
            var versionsTarget = _restTarget.Resolve("portfolios", request.ObjectId.ToString(), "versions");
            RestTarget target = RestUtils.EncodeQueryParams(versionsTarget, request);
            return target.Get<PortfolioHistoryResult>();
        }

        public PortfolioDocument Get(UniqueId uniqueId)
        {
            ArgumentChecker.NotNull(uniqueId, "uniqueId");
            var resp = _restTarget.Resolve("portfolios").Resolve(uniqueId.ObjectID.ToString()).Get<PortfolioDocument>();
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
                return new RemoteChangeManger(_restTarget.Resolve("portfolios", "changeManager"), _fudgeContext);
            }
        }
    }
}
