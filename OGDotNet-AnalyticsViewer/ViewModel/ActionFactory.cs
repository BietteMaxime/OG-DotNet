using System;
using System.Collections.Concurrent;

namespace OGDotNet.AnalyticsViewer.ViewModel
{
    internal class ActionFactory<TKey, TValue>
    {
        private readonly Func<TKey, TValue> _action;
        private readonly ConcurrentDictionary<TKey, TValue> _results = new ConcurrentDictionary<TKey, TValue>();

        public ActionFactory(Func<TKey, TValue> action)
        {
            _action = action;
        }

        public Func<TValue> GetAction(TKey key)
        {//TODO suck less
            return () => _results.GetOrAdd(key, _action);
        }
    }
}