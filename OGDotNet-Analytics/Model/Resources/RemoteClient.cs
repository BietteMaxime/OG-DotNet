//-----------------------------------------------------------------------
// <copyright file="RemoteClient.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    public class RemoteClient : DisposableBase
    {
        private readonly string _activeMQSpec;
        private readonly string _clientId;
        private readonly RestTarget _rest;

        private readonly HeartbeatSender _heartbeatSender;
        private readonly OpenGammaFudgeContext _fudgeContext;

        public RemoteClient(RestTarget userDataRest, string activeMQSpec, OpenGammaFudgeContext fudgeContext)
            : this(userDataRest, activeMQSpec, Environment.UserName, Guid.NewGuid().ToString(), fudgeContext)
        {
        }

        private RemoteClient(RestTarget userDataRest, string activeMQSpec, string username, string clientId, OpenGammaFudgeContext fudgeContext)
        {
            _clientId = clientId;
            _fudgeContext = fudgeContext;
            _rest = userDataRest.Resolve(username).Resolve("clients").Resolve(_clientId);
            _heartbeatSender = new HeartbeatSender(TimeSpan.FromMinutes(5), _rest.Resolve("heartbeat"));
            _activeMQSpec = activeMQSpec;
        }
        
        public RemoteSecurityMaster SecurityMaster
        {
            get
            {
                return new RemoteSecurityMaster(_rest.Resolve("securities"));
            }
        }

        public RemoteMarketDataSnapshotMaster MarketDataSnapshotMaster
        {
            get
            {
                return new RemoteMarketDataSnapshotMaster(_rest.Resolve("snapshots"), _activeMQSpec, _fudgeContext);
            }
        }

        public InterpolatedYieldCurveDefinitionMaster InterpolatedYieldCurveDefinitionMaster
        {
            get
            {
                return new InterpolatedYieldCurveDefinitionMaster(_rest.Resolve("interpolatedYieldCurveDefinitions"));
            }
        }

        public RemoteManagableViewDefinitionRepository ViewDefinitionRepository
        {
            get
            {
                return new RemoteManagableViewDefinitionRepository(_rest.Resolve("viewDefinitions"));
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