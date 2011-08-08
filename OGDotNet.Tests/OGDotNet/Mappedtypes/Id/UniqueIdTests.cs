// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UniqueIdTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using OGDotNet.Mappedtypes.Id;
using Xunit;
using Xunit.Extensions;

namespace OGDotNet.Tests.OGDotNet.Mappedtypes.Id
{
    public class UniqueIdTests
    {
        static readonly UniqueId[] ExpectedOrder = new[]
                                    {
                                        UniqueId.Of("A", "1"),
                                        UniqueId.Of("A", "2"),
                                        UniqueId.Of("A", "2", "A"),
                                        UniqueId.Of("A", "2", "B"),
                                        UniqueId.Of("B", "1"),
                                        UniqueId.Of("B", "2"),
                                    };

        [Fact]
        public void ComparableBehavesAsExpected()
        {
            for (int i = 0; i < ExpectedOrder.Length; i++)
            {
                var small = ExpectedOrder[i];

                Assert.Equal(0, small.CompareTo(small));
                Assert.Equal(0, small.CompareTo((object)small));

                for (int j = i + 1; j < ExpectedOrder.Length; j++)
                {
                    var big = ExpectedOrder[j];
                    Assert.InRange(small.CompareTo(big), int.MinValue, -1);
                    Assert.InRange(small.CompareTo((object)big), int.MinValue, -1);

                    Assert.InRange(big.CompareTo(small), 1, int.MaxValue);
                    Assert.InRange(big.CompareTo((object)small), 1, int.MaxValue);
                }
            }
        }

        [Fact]
        public void HashCodeBehavesAsExpected()
        {
            foreach (var uniqueIdentifier in ExpectedOrder)
            {
                Assert.Equal(1, ExpectedOrder.Where(e => e.GetHashCode() == uniqueIdentifier.GetHashCode()).Count());
                Assert.Equal(uniqueIdentifier.GetHashCode(), UniqueId.Of(uniqueIdentifier.Scheme, uniqueIdentifier.Value, uniqueIdentifier.Version).GetHashCode());
            }
        }

        [Fact]
        public void EquatableEqualsCodeBehavesAsExpected()
        {
            EqualsCodeBehavesAsExpected((a, b) => a.Equals(b));
        }

        [Fact]
        public void ObjectEqualsCodeBehavesAsExpected()
        {
            EqualsCodeBehavesAsExpected((a, b) => a.Equals((object)b));
            Assert.Throws<ArgumentException>(() => ExpectedOrder.First().CompareTo("SomeOtherType"));
            Assert.False(ExpectedOrder.First().Equals("SomeOtherType"));
        }

        [Fact]
        public void OperatorEqualsCodeBehavesAsExpected()
        {
            EqualsCodeBehavesAsExpected((a, b) => a == b);
        }

        private static void EqualsCodeBehavesAsExpected(Func<UniqueId, UniqueId, bool> equals)
        {
            foreach (var id in ExpectedOrder)
            {
                Assert.Equal(1, ExpectedOrder.Where(e => equals(e, id)).Count());
                Assert.True(equals(id, UniqueId.Of(id.Scheme, id.Value, id.Version)));
            }
        }

        [Fact]
        public void ParseWorks()
        {
            foreach (var uniqueIdentifier in ExpectedOrder)
            {
                Assert.Equal(uniqueIdentifier, UniqueId.Parse(uniqueIdentifier.ToString()));
            }
        }

        [Theory]
        [InlineData("A")]
        [InlineData("A~B~C~D~E")]
        public void WrongNumberOfSeparatorsFails(string uid)
        {
            Assert.Throws<ArgumentException>(() => UniqueId.Parse(uid));
        }
    }
}
