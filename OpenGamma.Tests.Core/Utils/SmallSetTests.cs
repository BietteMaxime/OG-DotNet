// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmallSetTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using OpenGamma.Util;

using Xunit;

namespace OpenGamma.Utils
{
    public class SmallSetTests
    {
        [Fact]
        public void BigSetSane()
        {
            var values = new[] { "A", "B" };
            var a = SmallSet<string>.Create(values);
            var b = new HashSet<string>(values);
            var c = SmallSet<string>.Create(b);

            Assert.IsNotType(typeof(SmallSet<string>), a);
            Assert.IsNotType(typeof(SmallSet<string>), c);
            Assert.True(a.SetEquals(b));
            Assert.True(b.SetEquals(a));
            Assert.True(b.SetEquals(c));
        }

        [Fact]
        public void SingletonEqualitySane()
        {
            var a = SmallSet<string>.Create("A");
            var b = SmallSet<string>.Create(new HashSet<string> {"A"});
            var c = SmallSet<string>.Create(new[] { "A" });
            var d = new HashSet<string> {"A"};

            Assert.IsType(typeof(SmallSet<string>), a);
            Assert.IsType(typeof(SmallSet<string>), b);
            Assert.IsType(typeof(SmallSet<string>), c);
            Assert.IsNotType(typeof(SmallSet<string>), d);

            var diff = SmallSet<string>.Create("B");

            Assert.Equal(a, b);
            Assert.True(a.SetEquals(b));
            Assert.Equal(a, c);
            Assert.Equal(b, c);
            Assert.True(a.SetEquals(d));
            Assert.True(d.SetEquals(a));
            Assert.True(d.SetEquals(b));
            Assert.True(d.SetEquals(c));

            Assert.NotEqual(a, diff);
            Assert.NotEqual(b, diff);
            Assert.NotEqual(c, diff);

            Assert.False(diff.SetEquals(a));
            Assert.False(a.SetEquals(diff));
            Assert.False(diff.SetEquals(b));
            Assert.False(diff.SetEquals(c));
            Assert.False(diff.SetEquals(d));
        }

        [Fact]
        public void CheckContains()
        {
            var a = SmallSet<string>.Create("A");
            Assert.True(a.Contains("A"));
            Assert.False(a.Contains("B"));
        }

        [Fact]
        public void SingletonReadonly()
        {
            var set = SmallSet<string>.Create("A");
            Assert.True(set.IsReadOnly);
            Assert.Throws<InvalidOperationException>(() => set.Remove("A"));
            Assert.Throws<InvalidOperationException>(() => set.Clear());
        }
    }
}
