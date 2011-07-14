//-----------------------------------------------------------------------
// <copyright file="JavaTypeMappingStrategyWithInterfaces.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Text;
using Fudge.Serialization;

namespace OGDotNet.Builders
{
    class JavaTypeMappingStrategyWithInterfaces : JavaTypeMappingStrategy
    {
        private readonly string _dotNetPrefix;
        readonly ConcurrentDictionary<string, Type> _getTypeCache = new ConcurrentDictionary<string, Type>();

        public JavaTypeMappingStrategyWithInterfaces(string dotNetPrefix, string javaPrefix)
            : base(dotNetPrefix, javaPrefix)
        {
            _dotNetPrefix = dotNetPrefix;
        }

        public override string GetName(Type type)
        {
            if (type.FullName.StartsWith(_dotNetPrefix+".javax"))
            {
                return type.FullName.Substring(_dotNetPrefix.Length + 1);
            }
            var javaName = base.GetName(type);
            
            if (type.IsInterface && type.Name.Length > 2 && type.Name[0] == 'I' && char.IsUpper(type.Name[1]))
            {
                var dotIndex = javaName.LastIndexOf('.');
                var stringBuilder = new StringBuilder(javaName);
                stringBuilder.Remove(dotIndex + 1, 1);
                stringBuilder[dotIndex + 1] = char.ToLowerInvariant(stringBuilder[dotIndex + 1]);
            }
            return javaName;
        }

        public override Type GetType(string name)
        {
            if (_getTypeCache.ContainsKey(name))
            {
                return _getTypeCache[name];
            }
            var ret = GetTypeImpl(name);
            // Cacheing null returns (e.g. Memoizer) would lose the dynamic behaviour
            if (ret != null) 
            {
                _getTypeCache[name] = ret;
            }
            return ret;
        }

        private Type GetTypeImpl(string name)
        {
            var ret = base.GetType(name);
            if (ret == null && name.Contains("."))
            {
                var interfaceName = new StringBuilder(name);
                interfaceName.Insert(name.LastIndexOf(".") + 1, 'I');
                ret = base.GetType(interfaceName.ToString());
            }
            return ret;
        }
    }
}
