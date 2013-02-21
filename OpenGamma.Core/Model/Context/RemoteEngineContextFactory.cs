// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteEngineContextFactory.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

using Fudge;

using OpenGamma.Fudge;
using OpenGamma.Util;

namespace OpenGamma.Model.Context
{
    public class RemoteEngineContextFactory : LoggingClassBase
    {
        private readonly OpenGammaFudgeContext _fudgeContext;
        private readonly Uri _rootUri;
        private readonly RestTarget _rootRest;

        // NOTE: don't do the blocking init in the constructor
        private readonly Lazy<IFudgeFieldContainer> _componentsMessage;
        private readonly Lazy<ComponentRepository> _componentRepository;

        public RemoteEngineContextFactory(OpenGammaFudgeContext fudgeContext, Uri rootUri)
        {
            _fudgeContext = fudgeContext;
            _rootUri = rootUri;
            _rootRest = new RestTarget(_fudgeContext, rootUri);

            _componentsMessage = new Lazy<IFudgeFieldContainer>(GetComponentsMessage);
            _componentRepository = new Lazy<ComponentRepository>(() => GetComponentRepository(_componentsMessage.Value));
        }

        private IFudgeFieldContainer GetComponentsMessage()
        {
            Logger.Info("Getting configuration info for {0}", _rootUri);
            var msg = _rootRest.Resolve("components").GetFudge();

            Logger.Debug("Components {0} {1}", _rootUri, msg);
            return msg;
        }

        public RemoteEngineContext CreateRemoteEngineContext()
        {
            return new RemoteEngineContext(_fudgeContext, _rootUri, _componentRepository.Value);
        }

        #region ConfigReading

        private ComponentRepository GetComponentRepository(IFudgeFieldContainer configMsg)
        {
            var componentInfos = new Dictionary<ComponentKey, ComponentInfo>();
            foreach (var userDataField in configMsg.GetMessage("infos"))
            {
                if (!(userDataField.Value is IFudgeFieldContainer))
                {
                    continue;
                }

                var component = (IFudgeFieldContainer)userDataField.Value;
                var uri = new Uri(_rootUri, component.GetString("uri"));
                var componentKey = new ComponentKey(component.GetString("type"), component.GetString("classifier"));
                Dictionary<string, string> attributes = component.GetMessage("attributes").ToDictionary(f => f.Name, f => (string) f.Value);
                componentInfos.Add(componentKey, new ComponentInfo(componentKey, uri, attributes));
            }

            return new ComponentRepository(componentInfos);
        }

        #endregion
    }
}