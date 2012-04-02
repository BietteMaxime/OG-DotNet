//-----------------------------------------------------------------------
// <copyright file="RemoteLiveMarketDataSourceRegistry.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using Fudge.Types;

namespace OGDotNet.Model.Resources
{
    public class RemoteNamedMarketDataSpecificationRepository
    {
        private readonly RestTarget _rest;

        public RemoteNamedMarketDataSpecificationRepository(RestTarget rest)
        {
            _rest = rest;
        }

        public IEnumerable<string> GetNames()
        {
            var fudgeMsg = _rest.Resolve("names").GetFudge();
            return fudgeMsg.Select(fudgeField => fudgeField.Value == IndicatorType.Instance ? null : (string)fudgeField.Value).ToList();
        }
    }
}