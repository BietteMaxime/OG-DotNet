//-----------------------------------------------------------------------
// <copyright file="RemoteViewProcessor.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace OGDotNet.Model.Resources
{
    public class RemoteViewProcessor
    {
        private readonly OpenGammaFudgeContext _fudgeContext;
        private readonly RestTarget _rest;
        private readonly string _activeMqSpec;

        public RemoteViewProcessor(OpenGammaFudgeContext fudgeContext, RestTarget rest, string activeMqSpec)
        {
            _fudgeContext = fudgeContext;
            _rest = rest;
            _activeMqSpec = activeMqSpec;
        }

        public IEnumerable<string> GetViewNames()
        {
            var fudgeMsg = _rest.Resolve("viewNames").GetFudge();

            return fudgeMsg.GetAllByOrdinal(1).Select(fudgeField => (string) fudgeField.Value);
        }

        public RemoteView GetView(string viewName)
        {
            return new RemoteView(_fudgeContext, _rest.Resolve("views").Resolve(viewName), _activeMqSpec, viewName);
        }
    }
}