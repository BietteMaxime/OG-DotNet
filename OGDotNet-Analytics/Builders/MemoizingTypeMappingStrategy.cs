//-----------------------------------------------------------------------
// <copyright file="MemoizingTypeMappingStrategy.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using Fudge.Serialization;
using OGDotNet.Utils;

namespace OGDotNet.Builders
{
    class MemoizingTypeMappingStrategy : IFudgeTypeMappingStrategy
    {
        readonly ConcurrentDictionary<string, Type> _getTypeCache = new ConcurrentDictionary<string, Type>();
        readonly Memoizer<Type, string> _getNameCache;

        private readonly IFudgeTypeMappingStrategy _inner;

        public MemoizingTypeMappingStrategy(IFudgeTypeMappingStrategy inner)
        {
            _inner = inner;
            _getNameCache = new Memoizer<Type, string>(GetNameImpl);
        }

        public string GetName(Type type)
        {
            return _getNameCache.Get(type);
        }

        private string GetNameImpl(Type arg)
        {
            return _inner.GetName(arg);
        }

        public Type GetType(string name)
        {
            Type ret;
            if (_getTypeCache.TryGetValue(name, out ret))
            {
                return ret;
            }
            ret = GetTypeImpl(name);
            // Cacheing null returns (e.g. Memoizer) would lose the dynamic behaviour
            if (ret != null)
            {
                _getTypeCache[name] = ret; //Repeated assignments will be matching
            }
            return ret;
        }

        private Type GetTypeImpl(string name)
        {
            return _inner.GetType(name);
        }
    }
}