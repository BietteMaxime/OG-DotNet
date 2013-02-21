// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComponentRepository.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace OpenGamma.Model.Context
{
    public class ComponentRepository
    {
        private readonly Dictionary<ComponentKey, ComponentInfo> _infoMap;

        public ComponentRepository(Dictionary<ComponentKey, ComponentInfo> infoMap)
        {
            _infoMap = infoMap;
        }

        public ComponentInfo GetComponentInfo(ComponentKey key)
        {
            ComponentInfo info;
            if (_infoMap.TryGetValue(key, out info))
            {
                return info;
            }

            throw new OpenGammaException(string.Format("ComponentRepository did not include service {0}, {1}", key, this));
        }
    }
}
