// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteConfigSource.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Fudge.Serialization;

using OpenGamma.Fudge;
using OpenGamma.Id;
using OpenGamma.Model;

namespace OpenGamma.Core.Config.Impl
{
    public class RemoteConfigSource
    {
        private readonly RestTarget _rest;
        private readonly OpenGammaFudgeContext _fudgeContext;

        public RemoteConfigSource(RestTarget rest, OpenGammaFudgeContext fudgeContext)
        {
            _rest = rest;
            _fudgeContext = fudgeContext;
        }

        public TConfig Get<TConfig>(string configName)
        {
            return Get<TConfig>(configName, null);
        }

        public TConfig Get<TConfig>(string configName, VersionCorrection versionCorrection)
        {
            if (versionCorrection == null)
            {
                versionCorrection = VersionCorrection.Latest;
            }

            var typeMapper = (IFudgeTypeMappingStrategy) _fudgeContext.GetProperty(ContextProperties.TypeMappingStrategyProperty);
            string mappedType = typeMapper.GetName(typeof(TConfig));
            return _rest.Resolve("configSearches").Resolve("single")
                .WithParam("name", configName)
                .WithParam("type", mappedType)
                .WithParam("versionCorrection", versionCorrection.ToString())
                .Get<TConfig>();
        }
    }
}
