// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteViewCycleReference.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Net;

using OpenGamma.Engine.View.Calc;
using OpenGamma.Financial.view.rest;
using OpenGamma.Util;

namespace OpenGamma.Model.Resources
{
    internal class RemoteViewCycleReference : DisposableBase, IEngineResourceReference<IViewCycle>
    {
        private readonly RestTarget _location;
        private readonly HeartbeatSender _heartbeatSender;

        public RemoteViewCycleReference(RestTarget location)
        {
            _location = location;
            _heartbeatSender = new HeartbeatSender(TimeSpan.FromSeconds(2.5), _location);
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
            try
            {
                _location.Delete();
            }
            catch (WebException e)
            {
                var httpWebResponse = (HttpWebResponse)e.Response;
                if (httpWebResponse == null)
                {
                    if (e.Status == WebExceptionStatus.ConnectFailure)
                    {
                        // LAP-71
                        return;
                    }

                    throw;
                }

                switch (httpWebResponse.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        break;
                    default:
                        throw;
                }
            }
        }
    }
}