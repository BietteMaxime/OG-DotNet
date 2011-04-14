//-----------------------------------------------------------------------
// <copyright file="Memoizer.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;

namespace OGDotNet.Utils
{
    /// <summary>
    /// NOTE: This Memoizer will grow unboundedly with the number of distinct arguments used
    /// </summary>
    public class Memoizer<TArg1, TArg2, TValue>
    {
        private readonly Memoizer<Tuple<TArg1, TArg2>, TValue> _inner;

        public Memoizer(Func<TArg1, TArg2, TValue> func)
        {
            _inner = new Memoizer<Tuple<TArg1, TArg2>, TValue>(a => func(a.Item1, a.Item2));
        }

        public TValue Get(TArg1 arg1, TArg2 arg2)
        {
            return _inner.Get(new Tuple<TArg1, TArg2>(arg1, arg2));
        }
    }

    /// <summary>
    /// NOTE: This Memoizer will grow unboundedly with the number of distinct arguments used
    /// </summary>
    public class Memoizer<TArg, TValue>
    {
        private readonly ConcurrentDictionary<TArg, TValue> _values = new ConcurrentDictionary<TArg, TValue>();
        private readonly Func<TArg, TValue> _func;

        public Memoizer(Func<TArg, TValue> func)
        {
            _func = func;
        }

        public TValue Get(TArg arg)
        {
            return _values.GetOrAdd(arg, _func);
        }
    }
}
