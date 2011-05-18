//-----------------------------------------------------------------------
// <copyright file="RemoteViewCycle.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.calc;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    internal class RemoteViewCycle : IViewCycle
    {
        private readonly RestTarget _location;
        private readonly OpenGammaFudgeContext _fudgeContext;

        public RemoteViewCycle(RestTarget location, OpenGammaFudgeContext fudgeContext)
        {
            _location = location;
            _fudgeContext = fudgeContext;
        }

        public UniqueIdentifier UniqueId
        {
            get
            {
                return _location.Resolve("id").Get<UniqueIdentifier>();
            }
        }

        public ViewComputationResultModel GetResultModel()
        {
            return _location.Resolve("result").Get<ViewComputationResultModel>();
        }

        public ComputationCacheResponse QueryComputationCaches(ComputationCacheQuery computationCacheQuery)
        {
            ArgumentChecker.NotNull(computationCacheQuery, "computationCacheQuery");
            string msgBase64 = _location.EncodeBean(computationCacheQuery);

            return _location.Resolve("queryCaches", new Tuple<string, string>("msg", msgBase64)).Get<ComputationCacheResponse>();
        }
    }
}