// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FinancialClient.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using OpenGamma.Fudge;
using OpenGamma.Master.Config;
using OpenGamma.Model;
using OpenGamma.Model.Resources;
using OpenGamma.Util;

namespace OpenGamma.Financial.User
{
    public class FinancialClient : DisposableBase
    {
        private readonly RestTarget _rest;

        private readonly HeartbeatSender _heartbeatSender;
        private readonly OpenGammaFudgeContext _fudgeContext;

        public FinancialClient(RestTarget rest, OpenGammaFudgeContext fudgeContext)
        {
            _rest = rest;
            _fudgeContext = fudgeContext;
            _heartbeatSender = new HeartbeatSender(TimeSpan.FromMinutes(5), _rest.Resolve("heartbeat"));
        }

        public RemoteMarketDataSnapshotMaster MarketDataSnapshotMaster
        {
            get
            {
                return new RemoteMarketDataSnapshotMaster(_rest.Resolve("snapshotMaster"));
            }
        }

        public InterpolatedYieldCurveDefinitionMaster InterpolatedYieldCurveDefinitionMaster
        {
            get
            {
                return new InterpolatedYieldCurveDefinitionMaster(_rest.Resolve("interpolatedYieldCurveDefinitionMaster"));
            }
        }

        public RemoteConfigMaster ConfigMaster
        {
            get
            {
                return new RemoteConfigMaster(_rest.Resolve("configMaster"));
            }
        }

        public RemotePositionMaster PositionMaster
        {
            get
            {
                return new RemotePositionMaster(_rest.Resolve("positionMaster"));
            }
        }

        public RemotePortfolioMaster PortfolioMaster
        {
            get
            {
                return new RemotePortfolioMaster(_rest.Resolve("portfolioMaster"));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _heartbeatSender.Dispose();
            }
        }
    }
}