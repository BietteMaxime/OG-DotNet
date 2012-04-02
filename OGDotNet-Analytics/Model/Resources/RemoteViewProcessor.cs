//-----------------------------------------------------------------------
// <copyright file="RemoteViewProcessor.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.LiveData;

namespace OGDotNet.Model.Resources
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

        public RemoteViewDefinitionRepository ViewDefinitionRepository
        {
            get
            {
                return new RemoteViewDefinitionRepository(_rest.Resolve("definitions"));
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

        public RemoteViewClient CreateClient(UserPrincipal userPrincipal)
        {
            var clientUri = _rest.Resolve("clients").Create(userPrincipal);

            return new RemoteViewClient(_fudgeContext, clientUri, _mqTemplate, this);
        }
        public RemoteViewClient CreateClient()
        {
            return CreateClient(UserPrincipal.DefaultUser);
        }

        public UniqueId GetUniqueId()
        {
            var restTarget = _rest.Resolve("id");
            return restTarget.Get<UniqueId>();
        }
    }
}