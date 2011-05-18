//-----------------------------------------------------------------------
// <copyright file="RemoteViewCycleReference.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using OGDotNet.Mappedtypes.engine.View.calc;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    internal class RemoteViewCycleReference : DisposableBase, IEngineResourceReference<IViewCycle>
    {
        private readonly RestTarget _location;
        private readonly OpenGammaFudgeContext _fudgeContext;
        private readonly HeartbeatSender _heartbeatSender;

        public RemoteViewCycleReference(RestTarget location, OpenGammaFudgeContext fudgeContext)
        {
            _location = location;
            _fudgeContext = fudgeContext;
            _heartbeatSender = new HeartbeatSender(TimeSpan.FromSeconds(5), _location);
        }

        public IViewCycle Value
        {
            get
            {
                return new RemoteViewCycle(_location.Resolve("resource"));
            }
        }

        protected override void Dispose(bool disposing)
        {
            _heartbeatSender.Dispose();
            _location.Delete();
        }
    }
}