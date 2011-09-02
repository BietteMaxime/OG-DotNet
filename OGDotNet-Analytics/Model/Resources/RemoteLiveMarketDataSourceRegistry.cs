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
    public class RemoteLiveMarketDataSourceRegistry
    {
        private readonly RestTarget _rest;

        public RemoteLiveMarketDataSourceRegistry(RestTarget rest)
        {
            _rest = rest;
        }

        public IEnumerable<string> GetDataSources()
        {
            var fudgeMsg = _rest.GetFudge();

            return fudgeMsg.Select(fudgeField => fudgeField.Value == IndicatorType.Instance ? null : (string)fudgeField.Value).ToList();
        }
    }
}