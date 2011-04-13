//-----------------------------------------------------------------------
// <copyright file="DictionaryExtensionMethods.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace OGDotNet.Utils
{
    static class DictionaryExtensionMethods
    {
        internal static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> dicta, IDictionary<TKey, TValue> dictb)
        {
            return Merge(dicta, dictb, (a,b) => b);
        }

        internal static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> dicta, IDictionary<TKey, TValue> dictb, Func<TValue, TValue, TValue> merge)
        {
            var ret = new Dictionary<TKey,TValue>();
            
            foreach (var key in dicta.Keys.Concat(dictb.Keys).Distinct())
            {
                TValue aValue;
                var haveAValue = dicta.TryGetValue(key, out aValue);
                TValue bValue;
                var haveBValue = dictb.TryGetValue(key, out bValue);

                if (haveAValue && haveBValue)
                    ret[key] = merge(aValue, bValue);
                else if (haveAValue)
                    ret[key] = aValue;
                else if (haveBValue)
                    ret[key] = bValue;
                else
                    throw new ArgumentException();
            }
            return ret;
        }

        public static IEnumerable<TRet> ProjectStructure<TKey, TValue, TRet>(this Dictionary<TKey, TValue> dictA, Dictionary<TKey, TValue> dictB,
            Func<TKey, TValue, TKey, TValue, TRet> matchingProjecter,
            Func<TKey, TValue, TRet> aOnlyProjecter,
            Func<TKey, TValue, TRet> bOnlyProjecter
            )
        {
            if (dictA.Comparer != dictB.Comparer)
                throw new ArgumentException();
            return
               dictA.Join(dictB, a => a.Key, b => b.Key, (a, b) => matchingProjecter(a.Key, a.Value, b.Key, b.Value), dictA.Comparer)

               .Concat(
                   dictB.Where(k => !dictA.ContainsKey(k.Key)).Select(k => bOnlyProjecter(k.Key, k.Value))
               )
               .Concat(
                   dictA.Where(k => !dictB.ContainsKey(k.Key)).Select(k => aOnlyProjecter(k.Key, k.Value))
               )
               ;
        }

        public static IEnumerable<TRet> ProjectStructure<TKey, TValue, TRet>(this IDictionary<TKey, TValue> dictA, IDictionary<TKey, TValue> dictB,
            Func<TKey, TValue, TValue, TRet> matchingProjecter,
            Func<TKey, TValue, TRet> aOnlyProjecter,
            Func<TKey, TValue, TRet> bOnlyProjecter
            )
        {
            return ProjectStructure((Dictionary<TKey, TValue>) dictA, (Dictionary<TKey, TValue>) dictB, (ka, va, kb, vb) => matchingProjecter(ka, va, vb), aOnlyProjecter, bOnlyProjecter);
        }
    }
}
