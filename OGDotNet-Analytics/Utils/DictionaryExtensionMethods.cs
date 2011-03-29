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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictA"></param>
        /// <param name="dictB"></param>
        /// <param name="updater"></param>
        /// <returns>true iff there is a structural change</returns>
        public static bool UpdateDictionaryFrom<TKey, TValue>(this IDictionary<TKey, TValue> dictA, IDictionary<TKey, TValue> dictB, Action<TValue, TValue> updater)
        {
            return UpdateDictionaryFrom(dictA, dictB, (a, b) => { updater(a, b); return false; });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictA"></param>
        /// <param name="dictB"></param>
        /// <param name="updater"></param>
        /// <returns>true iff the updater ever returns true or there is a structural change</returns>
        public static bool UpdateDictionaryFrom<TKey, TValue>(this IDictionary<TKey, TValue> dictA, IDictionary<TKey, TValue> dictB, Func<TValue, TValue,bool> updater)
        {
            bool dirty = false;
            var enumerable = Enumerable.Join(dictA, dictB, a => a.Key, b => b.Key, (a, b) => Tuple.Create(a.Value, b.Value));
            foreach (var tuple in enumerable)
            {
                bool itemDirty = updater(tuple.Item1, tuple.Item2);
                dirty |= itemDirty;
            }

            foreach (var keyToRemove in Enumerable.Where(dictA.Keys, k => !dictB.ContainsKey(k)).ToList())
            {
                dirty = true;
                dictA.Remove(keyToRemove);
            }
            foreach (var kvpToAdd in Enumerable.Where(dictB, kvp => !dictA.ContainsKey(kvp.Key)).ToList())
            {
                dirty = true;
                dictA.Add(kvpToAdd.Key,kvpToAdd.Value);
            }

            return dirty;
        }
    }
}
