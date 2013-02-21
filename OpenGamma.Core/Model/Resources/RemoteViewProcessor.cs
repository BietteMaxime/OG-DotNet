// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteViewProcessor.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Core.Config.Impl;
using OpenGamma.Financial.view.rest;
using OpenGamma.Fudge;
using OpenGamma.Id;
using OpenGamma.LiveData;

namespace OpenGamma.Model.Resources
{
    public class RemoteViewProcessor
    {
        private readonly OpenGammaFudgeContext _fudgeContext;
        private readonly RestTarget _rest;
        private readonly MQTemplate _mqTemplate;

        public RemoteViewProcessor(OpenGammaFudgeContext fudgeContext, RestTarget rest, MQTemplate mqTemplate)
        {
            _fudgeContext = fudgeContext;
            _rest = rest;
            _mqTemplate = mqTemplate;
        }

        public RemoteConfigSource ConfigSource
        {
            get
            {
                return new RemoteConfigSource(_rest.Resolve("configSource"), _fudgeContext);
            }
        }

        public RemoteNamedMarketDataSpecificationRepository LiveMarketDataSourceRegistry
        {
            get
            {
                return new RemoteNamedMarketDataSpecificationRepository(_rest.Resolve("namedMarketDataSpecRepository"));
            }
        }

        public RemoteMarketDataSnapshotter MarketDataSnapshotter
        {
            get
            {
                return new RemoteMarketDataSnapshotter(_rest.Resolve("marketDataSnapshotter"));
            }
        }

        public RemoteViewClient CreateViewClient(UserPrincipal userPrincipal)
        {
            var clientUri = _rest.Resolve("clients").Create(userPrincipal);

            return new RemoteViewClient(_fudgeContext, clientUri, _mqTemplate, this);
        }

        public RemoteViewClient CreateViewClient()
        {
            return CreateViewClient(UserPrincipal.DefaultUser);
        }

        public UniqueId GetUniqueId()
        {
            var restTarget = _rest.Resolve("id");
            return restTarget.Get<UniqueId>();
        }
    }
}