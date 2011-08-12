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
using OGDotNet.Mappedtypes;
using OGDotNet.Utils;

namespace OGDotNet.Builders
{
    class JavaTypeMappingStrategyWithInterfaces : JavaTypeMappingStrategy
    {
        private readonly string _dotNetPrefix;
        readonly ConcurrentDictionary<string, Type> _getTypeCache = new ConcurrentDictionary<string, Type>();
        readonly Memoizer<Type, string> _getNameCache;

        public JavaTypeMappingStrategyWithInterfaces(string dotNetPrefix, string javaPrefix)
            : base(dotNetPrefix, javaPrefix)
        {
            _getNameCache = new Memoizer<Type, string>(GetNameImpl);
            _dotNetPrefix = dotNetPrefix;
        }

        public override string GetName(Type type)
        {
            return _getNameCache.Get(type);
        }

        private string GetNameImpl(Type type)
        {
            string javaxPrefix = _dotNetPrefix + ".JavaX";
            if (type.FullName.StartsWith(javaxPrefix))
            {
                StringBuilder name = new StringBuilder("javax").Append(type.FullName.Substring(javaxPrefix.Length));
                int end = LastIndexOf(name, '.');
                for (int i = 0; i < end; i++)
                {
                    name[i] = char.ToLowerInvariant(name[i]);
                }
                return name.ToString();
            }
            var javaName = base.GetName(type);

            if (type.IsInterface && type.Name.Length > 2 && type.Name[0] == 'I' && char.IsUpper(type.Name[1]))
            {
                var dotIndex = javaName.LastIndexOf('.');
                var stringBuilder = new StringBuilder(javaName);
                stringBuilder.Remove(dotIndex + 1, 1);
                return stringBuilder.ToString();
            }
            return javaName;
        }

        private static int LastIndexOf(StringBuilder sb, char c)
        {
            for (int i = sb.Length - 1; i >= 0; i--)
            {
                if (sb[i] == c)
                {
                    return i;
                }
            }
            return -1;
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
                CheckRoundTrip(name, ret);
            }
            else if (name.StartsWith("javax"))
            {
                CheckRoundTrip(name, ret);
            }
            return ret;
        }

        private void CheckRoundTrip(string name, Type ret)
        {
            if (ret != null && GetName(ret) != name)
            {
                throw new OpenGammaException("Type cannot be roundtripped");
            }
        }
    }
}
