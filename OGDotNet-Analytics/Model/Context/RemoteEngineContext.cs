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

        internal RemoteEngineContext(OpenGammaFudgeContext fudgeContext, Uri rootUri, string activeMQSpec, IDictionary<string,Uri> serviceUris)
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
                return new RemoteViewProcessor(_fudgeContext, new RestTarget(_fudgeContext,  _serviceUris["viewProcessor"]), _activeMQSpec);
            }
        }

        public IFinancialSecuritySource SecuritySource
        {
            get {
                return new RemoteSecuritySource(_fudgeContext, new RestTarget(_fudgeContext,  _serviceUris["securitySource"]));
            }
        }

        public RemoteSecurityMaster SecurityMaster
        {//TODO this is a hack, should I even be exposing this?
            get
            {
                return new RemoteSecurityMaster(new RestTarget(_fudgeContext,  _serviceUris["securitySource"].ToString().Replace("securitySource", "securityMaster")));
            }
        }

        public RemoteInterpolatedYieldCurveSpecificationBuilder InterpolatedYieldCurveSpecificationBuilder
        {
            get
            {
                return new RemoteInterpolatedYieldCurveSpecificationBuilder(_fudgeContext, new RestTarget(_fudgeContext, _serviceUris["interpolatedYieldCurveSpecificationBuilder"]));
            }
        }

        public RemoteHistoricalDataSource HistoricalDataSource
        {
            get
            {
                return new RemoteHistoricalDataSource(_fudgeContext, new RestTarget(_fudgeContext, _serviceUris["historicalData"]));
            }
        }

        public RemoteCurrencyMatrixSource CurrencyMatrixSource
        {
            get
            {
                return new RemoteCurrencyMatrixSource(_fudgeContext, new RestTarget(_fudgeContext, _serviceUris["currencyMatrixSource"]));
            }
        }
    }
}