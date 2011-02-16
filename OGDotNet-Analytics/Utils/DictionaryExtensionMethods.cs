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
    }
}
