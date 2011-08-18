//-----------------------------------------------------------------------
// <copyright file="SmallSet.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OGDotNet.Utils
{
    public class SmallSet<T> : ISet<T>
    {
        private readonly T _value;

        private SmallSet(T value)
        {
            _value = value;
        }

        public static ISet<T> Create(HashSet<T> values)
        {
            switch (values.Count)
            {
                case 0:
                    throw new ArgumentOutOfRangeException();
                case 1:
                    return Create(values.Single());
                default:
                    return values;
            }
        }
        public static ISet<T> Create(IEnumerable<T> values)
        {
            List<T> list = values.ToList();
            switch (list.Count)
            {
                case 0:
                    throw new ArgumentOutOfRangeException();
                case 1:
                    return Create(list[0]);
                default:
                    return new HashSet<T>(list);
            }
        }

        public static ISet<T> Create(T value)
        {
            return new SmallSet<T>(value);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }
        private IEnumerable<T> Enumerate()
        {
            yield return _value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            throw new InvalidOperationException();
        }
            
        bool ISet<T>.Add(T item)
        {
            throw new InvalidOperationException();
        }
            
        public void UnionWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            var small = other as SmallSet<T>;

            if (small != null)
            {
                return small._value.Equals(_value);
            }
            else
            {
                return other.Any(v => v.Equals(_value));
            }
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            var smallSet = other as SmallSet<T>;
            if (smallSet != null)
            {
                return smallSet._value.Equals(_value);
            }

            bool done = false;
            foreach (var entry in other)
            {
                if (done)
                {
                    return false;
                }
                done = true;
                if (!_value.Equals(entry))
                {
                    return false;
                }
            }

            return done;
        }

        public void Clear()
        {
            throw new InvalidOperationException();
        }

        public bool Contains(T item)
        {
            return _value.Equals(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            array[arrayIndex] = _value;
        }

        public bool Remove(T item)
        {
            throw new InvalidOperationException();
        }

        public int Count
        {
            get { return 1; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }
    }
}