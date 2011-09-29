//-----------------------------------------------------------------------
// <copyright file="RemoteEngineContext.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using OGDotNet.Model.Resources;

namespace OGDotNet.Model.Context
{
    public class RemoteEngineContext
    {
        private readonly OpenGammaFudgeContext _fudgeContext;
        private readonly Uri _rootUri;
        private readonly string _activeMQSpec;
        private readonly IDictionary<string, Uri> _serviceUris;

        internal RemoteEngineContext(OpenGammaFudgeContext fudgeContext, Uri rootUri, string activeMQSpec, IDictionary<string, Uri> serviceUris)
        {
            _fudgeContext = fudgeContext;
            _rootUri = rootUri;
            _activeMQSpec = activeMQSpec;
            _serviceUris = serviceUris;
        }

        public Uri RootUri
        {
            get { return _rootUri; }
        }

        public MQTemplate MQTemplate
        {
            get { return new MQTemplate(_activeMQSpec); }
        }

        public OpenGammaFudgeContext FudgeContext
        {
            get { return _fudgeContext; }
        }

        private RestTarget GetTarget(string service)
        {
            return new RestTarget(_fudgeContext, _serviceUris[service]);
        }

        public RemoteClient CreateUserClient()
        {
            return new RemoteClient(GetTarget("userData"), _activeMQSpec, FudgeContext);
        }

        public RemoteViewProcessor ViewProcessor
        {
            get
            {
                return new RemoteViewProcessor(_fudgeContext, GetTarget("viewProcessor"), MQTemplate);
            }
        }

        public RemoteMarketDataSnapshotter MarketDataSnapshotter
        {
            get
            {
                return new RemoteMarketDataSnapshotter(GetTarget("marketDataSnapshotter"));
            }
        }

        public IFinancialSecuritySource SecuritySource
        {
            get
            {
                return new RemoteSecuritySource(GetTarget("securitySource"));
            }
        }

        public RemoteSecurityMaster SecurityMaster
        {
            get
            {
                return new RemoteSecurityMaster(GetTarget("sharedSecurityMaster"));
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
                return new RemoteMarketDataSnapshotMaster(GetTarget("sharedMarketDataSnapshotMaster"), _activeMQSpec, FudgeContext);
            }
        }

        public RemoteInterpolatedYieldCurveSpecificationBuilder InterpolatedYieldCurveSpecificationBuilder
        {
            get
            {
                return new RemoteInterpolatedYieldCurveSpecificationBuilder(GetTarget("interpolatedYieldCurveSpecificationBuilder"));
            }
        }

        public RemoteHistoricalTimeSeriesSource HistoricalTimeSeriesSource
        {
            get
            {
                return new RemoteHistoricalTimeSeriesSource(_fudgeContext, GetTarget("historicalTimeSeriesSource"));
            }
        }

        public RemoteCurrencyMatrixSource CurrencyMatrixSource
        {
            get
            {
                return new RemoteCurrencyMatrixSource(GetTarget("currencyMatrixSource"));
            }
        }

        public RemoteVolatilityCubeDefinitionSource VolatilityCubeDefinitionSource
        {
            get
            {
                return new RemoteVolatilityCubeDefinitionSource(GetTarget("volatilityCubeDefinitionSource"));
            }
        }

        public RemoteAvailableOutputs RemoteAvailableOutputs
        {
            get
            {
                return new RemoteAvailableOutputs(new RestTarget(_fudgeContext, _rootUri).Resolve("availableOutputs")); //TODO less hard coded
            }
        }

        public RemotePortfolioMaster PortfolioMaster
        {
            get
            {
                return new RemotePortfolioMaster(GetTarget("sharedPortfolioMaster"));
            }
        }
    }
}