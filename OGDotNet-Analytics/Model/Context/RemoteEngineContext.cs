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

        public RemoteClient CreateUserClient()
        {
            return new RemoteClient(new RestTarget(_fudgeContext, _serviceUris["userData"]));
        }

        public RemoteViewProcessor ViewProcessor
        {
            get
            {
                return new RemoteViewProcessor(_fudgeContext, new RestTarget(_fudgeContext, _serviceUris["viewProcessor"]), _activeMQSpec);
            }
        }

        public IFinancialSecuritySource SecuritySource
        {
            get
            {
                return new RemoteSecuritySource(new RestTarget(_fudgeContext, _serviceUris["securitySource"]));
            }
        }

        public RemoteSecurityMaster SecurityMaster
        {
            get
            {
                return new RemoteSecurityMaster(new RestTarget(_fudgeContext, _serviceUris["sharedSecurityMaster"]));
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
                return new RemoteMarketDataSnapshotMaster(new RestTarget(_fudgeContext, _serviceUris["sharedMarketDataSnapshotMaster"]));
            }
        }

        public RemoteInterpolatedYieldCurveSpecificationBuilder InterpolatedYieldCurveSpecificationBuilder
        {
            get
            {
                return new RemoteInterpolatedYieldCurveSpecificationBuilder(new RestTarget(_fudgeContext, _serviceUris["interpolatedYieldCurveSpecificationBuilder"]));
            }
        }

        public RemoteHistoricalTimeSeriesSource HistoricalTimeSeriesSource
        {
            get
            {
                return new RemoteHistoricalTimeSeriesSource(_fudgeContext, new RestTarget(_fudgeContext, _serviceUris["historicalTimeSeriesSource"]));
            }
        }

        public RemoteCurrencyMatrixSource CurrencyMatrixSource
        {
            get
            {
                return new RemoteCurrencyMatrixSource(new RestTarget(_fudgeContext, _serviceUris["currencyMatrixSource"]));
            }
        }

        public RemoteVolatilityCubeDefinitionSource VolatilityCubeDefinitionSource
        {
            get
            {
                return new RemoteVolatilityCubeDefinitionSource(new RestTarget(_fudgeContext, _serviceUris["volatilityCubeDefinitionSource"]));
            }
        }
    }
}