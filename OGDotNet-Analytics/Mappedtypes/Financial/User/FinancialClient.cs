//-----------------------------------------------------------------------
// <copyright file="FinancialClient.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using OGDotNet.Model;
using OGDotNet.Model.Resources;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.Financial.User
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
                return new RemoteMarketDataSnapshotMaster(_rest.Resolve("snapshotMaster"), _fudgeContext);
            }
        }

        public InterpolatedYieldCurveDefinitionMaster InterpolatedYieldCurveDefinitionMaster
        {
            get
            {
                return new InterpolatedYieldCurveDefinitionMaster(_rest.Resolve("interpolatedYieldCurveDefinitionMaster"));
            }
        }

        public RemoteManagableViewDefinitionRepository ViewDefinitionRepository
        {
            get
            {
                return new RemoteManagableViewDefinitionRepository(_rest.Resolve("viewDefinitionMaster"));
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