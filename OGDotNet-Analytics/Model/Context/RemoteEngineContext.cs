//-----------------------------------------------------------------------
// <copyright file="RemoteEngineContext.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using OGDotNet.Mappedtypes.Financial.User;
using OGDotNet.Model.Resources;

namespace OGDotNet.Model.Context
{
    public class RemoteEngineContext
    {
        private readonly OpenGammaFudgeContext _fudgeContext;
        private readonly Uri _rootUri;
        private readonly ComponentRepository _repository;

        internal RemoteEngineContext(OpenGammaFudgeContext fudgeContext, Uri rootUri, ComponentRepository repository)
        {
            _fudgeContext = fudgeContext;
            _rootUri = rootUri;
            _repository = repository;
        }

        public Uri RootUri
        {
            get { return _rootUri; }
        }

        public OpenGammaFudgeContext FudgeContext
        {
            get { return _fudgeContext; }
        }

        private RestTarget GetTarget(string service, string classifier = "main")
        {
            var componentInfo = _repository.GetComponentInfo(new ComponentKey(service, classifier));
            return GetTarget(componentInfo);
        }

        private RestTarget GetTarget(ComponentInfo componentInfo)
        {
            return new RestTarget(_fudgeContext, componentInfo.Uri);
        }

        public static MQTemplate GetMQTemplate(ComponentInfo componentInfo)
        {
            var template = new MQTemplate(componentInfo.Attributes["jmsBrokerUri"]);
            return template;
        }

        public static MQTopicTemplate GetMQTopic(ComponentInfo componentInfo)
        {
            var template = GetMQTemplate(componentInfo);
            return new MQTopicTemplate(template, componentInfo.Attributes["jmsChangeManagerTopic"]);
        }

        public FinancialUser FinancialUser
        {
            get { return GetFinancialUser(Environment.UserName); }
        }

        private FinancialUser GetFinancialUser(string userName)
        {
            var restTarget = GetTarget("com.opengamma.financial.user.FinancialUserManager");
            return new FinancialUser(_fudgeContext, restTarget.Resolve("users", userName), userName);
        }

        public FinancialClient CreateFinancialClient()
        {
            return FinancialUser.CreateClient();
        }

        public RemoteViewProcessor ViewProcessor
        {
            get
            {
                var componentInfo = _repository.GetComponentInfo(new ComponentKey("com.opengamma.engine.view.ViewProcessor", "main"));
                var viewProcessorId = componentInfo.Attributes["viewProcessorId"];
                var restTarget = GetTarget(componentInfo).Resolve(viewProcessorId);
                return new RemoteViewProcessor(_fudgeContext, restTarget, GetMQTemplate(componentInfo));
            }
        }

        public IFinancialSecuritySource SecuritySource
        {
            get
            {
                return new RemoteSecuritySource(GetTarget("com.opengamma.core.security.SecuritySource", "combined"));
            }
        }

        public RemoteSecurityMaster SecurityMaster
        {
            get
            {
                return new RemoteSecurityMaster(GetTarget("com.opengamma.master.security.SecurityMaster", "central"));
            }
        }

        public MarketDataSnapshotManager MarketDataSnapshotManager
        {
            get
            {
                return new MarketDataSnapshotManager(this);
            }
        }
        public RemoteMarketDataSnapshotMaster MarketDataSnapshotMaster
        {
            get
            {
                return new RemoteMarketDataSnapshotMaster(GetTarget("com.opengamma.master.marketdatasnapshot.MarketDataSnapshotMaster", "central"), FudgeContext);
            }
        }

        public RemoteInterpolatedYieldCurveSpecificationBuilder InterpolatedYieldCurveSpecificationBuilder
        {
            get
            {
                return new RemoteInterpolatedYieldCurveSpecificationBuilder(GetTarget("com.opengamma.financial.analytics.ircurve.InterpolatedYieldCurveSpecificationBuilder", "shared"));
            }
        }

        public RemoteHistoricalTimeSeriesSource HistoricalTimeSeriesSource
        {
            get
            {
                return new RemoteHistoricalTimeSeriesSource(_fudgeContext, GetTarget("com.opengamma.core.historicaltimeseries.HistoricalTimeSeriesSource", "shared"));
            }
        }

        public RemoteCurrencyMatrixSource CurrencyMatrixSource
        {
            get
            {
                return new RemoteCurrencyMatrixSource(GetTarget("com.opengamma.financial.currency.CurrencyMatrixSource", "shared"));
            }
        }

        public RemoteVolatilityCubeDefinitionSource VolatilityCubeDefinitionSource
        {
            get
            {
                return new RemoteVolatilityCubeDefinitionSource(GetTarget("com.opengamma.financial.analytics.volatility.cube.VolatilityCubeDefinitionSource", "combined"));
            }
        }

        public RemoteAvailableOutputs RemoteAvailableOutputs
        {
            get
            {
                return new RemoteAvailableOutputs(GetTarget("com.opengamma.engine.view.helper.AvailableOutputsProvider"));
            }
        }

        public RemotePortfolioMaster PortfolioMaster
        {
            get
            {
                var componentInfo = _repository.GetComponentInfo(new ComponentKey("com.opengamma.master.portfolio.PortfolioMaster", "central"));
                return new RemotePortfolioMaster(GetTarget(componentInfo), FudgeContext);
            }
        }
    }
}