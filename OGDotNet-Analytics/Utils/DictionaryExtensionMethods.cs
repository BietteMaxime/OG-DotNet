//-----------------------------------------------------------------------
// <copyright file="DictionaryExtensionMethods.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OGDotNet.Utils
{
    static class DictionaryExtensionMethods
    {
        public static IEnumerable<TRet> ProjectStructure<TKey, TValue, TRet>(this IDictionary<TKey, TValue> dictA, IDictionary<TKey, TValue> dictB,
            Func<TKey, TValue, TKey, TValue, TRet> matchingProjecter,
            Func<TKey, TValue, TRet> aOnlyProjecter,
            Func<TKey, TValue, TRet> bOnlyProjecter
            )
        {
            var aComparer = GetComparer(dictA);
            var bComparer = GetComparer(dictB);
            if (aComparer != bComparer)
                throw new ArgumentException("Automagically found comparers differed");
            return ProjectStructure(dictA, dictB, aComparer, matchingProjecter, bOnlyProjecter, aOnlyProjecter);
        }

        public static IEnumerable<TRet> ProjectStructure<TKey, TValue, TRet>(this IDictionary<TKey, TValue> dictA, IDictionary<TKey, TValue> dictB,
            IEqualityComparer<TKey> aComparer, 
            Func<TKey, TValue, TKey, TValue, TRet> matchingProjecter,
            Func<TKey, TValue, TRet> bOnlyProjecter,
            Func<TKey, TValue, TRet> aOnlyProjecter)
        {
            return
                dictA.Join(dictB, a => a.Key, b => b.Key, (a, b) => matchingProjecter(a.Key, a.Value, b.Key, b.Value), aComparer)
                    .Concat(
                        dictB.Where(k => !dictA.ContainsKey(k.Key)).Select(k => bOnlyProjecter(k.Key, k.Value))
                    )
                    .Concat(
                        dictA.Where(k => !dictB.ContainsKey(k.Key)).Select(k => aOnlyProjecter(k.Key, k.Value))
                    );
        }

        public static IEnumerable<TRet> ProjectStructure<TKey, TValue, TRet>(this IDictionary<TKey, TValue> dictA, IDictionary<TKey, TValue> dictB,
            Func<TKey, TValue, TValue, TRet> matchingProjecter,
            Func<TKey, TValue, TRet> aOnlyProjecter,
            Func<TKey, TValue, TRet> bOnlyProjecter
            )
        {
            return ProjectStructure(dictA, dictB, (ka, va, kb, vb) => matchingProjecter(ka, va, vb), aOnlyProjecter, bOnlyProjecter);
        }

        private static IEqualityComparer<TKey> GetComparer<TKey, TValue>(IDictionary<TKey, TValue> dict)
        {
            var dictionary = dict as Dictionary<TKey, TValue>;
            if (dictionary != null)
            {
                return dictionary.Comparer;
            }
            var concDictionary = dict as ConcurrentDictionary<TKey, TValue>;
            if (concDictionary != null)
            {
                return ConcDictionaryHandler<TKey, TValue>.GetComparer(concDictionary);
            }
            
            throw new ArgumentException("Couldn't find comparer automagically");
        }

        private static class ConcDictionaryHandler<TKey, TValue>
        {
            static readonly FieldInfo ComparerField = typeof(ConcurrentDictionary<TKey, TValue>).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(
                f => f.FieldType == typeof(IEqualityComparer<TKey>)).Single();

            public static IEqualityComparer<TKey> GetComparer(ConcurrentDictionary<TKey, TValue> dict)
            {
                return (IEqualityComparer<TKey>) ComparerField.GetValue(dict);
            }
        }
    }
}
