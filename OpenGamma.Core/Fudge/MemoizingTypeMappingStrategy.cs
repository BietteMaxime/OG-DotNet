// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MemoizingTypeMappingStrategy.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Threading;

using Fudge.Serialization;

using OpenGamma.Util;

namespace OpenGamma.Fudge
{
    public class MemoizingTypeMappingStrategy : IFudgeTypeMappingStrategy
    {
        readonly Memoizer<string, Type> _getTypeCache;
        readonly Memoizer<Type, string> _getNameCache;

        private readonly IFudgeTypeMappingStrategy _inner;

        public MemoizingTypeMappingStrategy(IFudgeTypeMappingStrategy inner)
        {
            _inner = inner;
            _getNameCache = new Memoizer<Type, string>(GetNameImpl);
            _getTypeCache = new Memoizer<string, Type>(GetTypeImpl);

            var weakRef = new WeakReference(this);
            HookUpWeakClearingDelegate(weakRef);
        }

        private static void HookUpWeakClearingDelegate(WeakReference weakRef)
        {
            AssemblyLoadEventHandler ret = null;
            ret = delegate(object sender, AssemblyLoadEventArgs args)
                      {
                          var target = (MemoizingTypeMappingStrategy) weakRef.Target;
                          if (target == null)
                          {
                              // Unhook us, multiple unhooks is fine
                              AppDomain.CurrentDomain.AssemblyLoad -= ret;
                          }
                          else
                          {
                              // Since we cache even null returns but we would like to keep the the dynamic behaviour
                              // If the type request is being proccessed whilst this is called then we might cache the old value
                              // but that would be mad
                              target._getTypeCache.Clear();
                          }
                      };
            Thread.MemoryBarrier();
            AppDomain.CurrentDomain.AssemblyLoad += ret;
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
            return _getTypeCache.Get(name);
        }

        private Type GetTypeImpl(string name)
        {
            return _inner.GetType(name);
        }
    }
}