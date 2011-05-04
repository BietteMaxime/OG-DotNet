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
        private readonly string _clientId;
        private readonly RestTarget _rest;

        private readonly HeartbeatSender _heartbeatSender;

        public RemoteClient(RestTarget userDataRest)
            : this(userDataRest, Environment.UserName, Guid.NewGuid().ToString())
        {
        }

        private RemoteClient(RestTarget userDataRest, string username, string clientId)
        {
            _clientId = clientId;
            _rest = userDataRest.Resolve(username).Resolve("clients").Resolve(_clientId);

            _heartbeatSender = new HeartbeatSender(TimeSpan.FromMinutes(5), _rest.Resolve("heartbeat"));
        }
        
        public RemoteSecurityMaster SecurityMaster
        {
            get
            {
                return new RemoteSecurityMaster(_rest.Resolve("securities"));
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
                return new RemoteManagableViewDefinitionRepository(_rest);
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