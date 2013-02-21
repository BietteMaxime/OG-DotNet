// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JavaTypeMappingStrategyWithInterfaces.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

using Fudge.Serialization;
using OpenGamma.Financial.Ccy;
using OpenGamma.Master.Config;

namespace OpenGamma.Fudge
{
    class OpenGammaTypeMappingStrategy : JavaTypeMappingStrategy
    {
        private readonly string _dotNetPrefix;
        private readonly IDictionary<string, Type> _explicitTypesFromJava = new Dictionary<string, Type>();
        private readonly IDictionary<Type, string> _explicitTypesToJava = new Dictionary<Type, string>();

        public OpenGammaTypeMappingStrategy(string dotNetPrefix, string javaPrefix)
            : base(dotNetPrefix, javaPrefix)
        {
            _dotNetPrefix = dotNetPrefix;
            AddExplicitMapping("com.opengamma.master.config.ConfigDocument", typeof(ConfigDocument<>));
            AddExplicitMapping("com.opengamma.financial.currency.CurrencyMatrix", typeof(CurrencyMatrix));
        }
        
        private void AddExplicitMapping(string javaType, Type type)
        {
            _explicitTypesFromJava.Add(javaType, type);
            _explicitTypesToJava.Add(type, javaType);
        }

        public override string GetName(Type type)
        {
            if (_explicitTypesToJava.ContainsKey(type))
            {
                return _explicitTypesToJava[type];
            }
            if (type.IsGenericType && _explicitTypesToJava.ContainsKey(type.GetGenericTypeDefinition()))
            {
                return _explicitTypesToJava[type.GetGenericTypeDefinition()];
            }
            string javaxPrefix = _dotNetPrefix + ".JavaX";
            var typeFullName = type.FullName;
            if (typeFullName.StartsWith(javaxPrefix))
            {
                StringBuilder name = new StringBuilder("javax").Append(typeFullName.Substring(javaxPrefix.Length));
                int end = LastIndexOf(name, '.');
                for (int i = 0; i < end; i++)
                {
                    name[i] = char.ToLowerInvariant(name[i]);
                }

                return name.ToString();
            }

            var javaName = base.GetName(type);

            var typeName = type.Name;
            if (type.IsInterface && typeName.Length > 2 && typeName[0] == 'I' && char.IsUpper(typeName[1]))
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
            var ret = base.GetType(name);
            if (ret == null)
            {
                if (_explicitTypesFromJava.ContainsKey(name))
                {
                    ret = _explicitTypesFromJava[name];
                    CheckRoundTrip(name, ret);
                }
                else if (name.Contains("."))
                {
                    var interfaceName = new StringBuilder(name);
                    interfaceName.Insert(name.LastIndexOf('.') + 1, 'I');
                    ret = base.GetType(interfaceName.ToString());
                    CheckRoundTrip(name, ret);
                }
            }
            else if (name.StartsWith("javax"))
            {
                CheckRoundTrip(name, ret);
            }

            return ret;
        }

        private void CheckRoundTrip(string name, Type ret)
        {
            if (ret == null)
            {
                return;
            }
            if (GetName(ret) != name)
            {
                throw new OpenGammaException("Type cannot be roundtripped");
            }
        }
    }
}
