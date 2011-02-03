using System;
using System.Collections.Generic;

namespace OGDotNet.Utils
{
    public class Memoizer<TArg1,TArg2, TValue>
    {
        private readonly Memoizer<Tuple<TArg1, TArg2>, TValue> _inner;

        public Memoizer(Func<TArg1,TArg2,TValue> func)
        {
            _inner = new Memoizer<Tuple<TArg1, TArg2>, TValue>(a => func(a.Item1, a.Item2));
        }

        public TValue Get(TArg1 arg1, TArg2 arg2)
        {
            return _inner.Get(new Tuple<TArg1, TArg2>(arg1, arg2));
        }
    }

    /// <summary>
    /// TODO fix leak
    /// </summary>
    public class Memoizer<TArg,TValue>
    {
        private readonly Dictionary<TArg,TValue> _values = new Dictionary<TArg, TValue>();
        private readonly Func<TArg, TValue> _func;

        public Memoizer(Func<TArg,TValue> func)
        {
            _func = func;
        }


        public TValue Get(TArg arg)
        {
            TValue ret;
            if (! _values.TryGetValue(arg, out ret))
            {
                ret = _func(arg);
                _values[arg] = ret;
            }
            return ret;
        }
    }
}
