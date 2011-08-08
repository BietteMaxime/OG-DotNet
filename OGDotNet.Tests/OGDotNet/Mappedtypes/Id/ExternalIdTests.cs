//-----------------------------------------------------------------------
// <copyright file="ExternalIdTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using OGDotNet.Mappedtypes.Id;
using Xunit;
using Xunit.Extensions;

namespace OGDotNet.Tests.OGDotNet.Mappedtypes.Id
{
    public class ExternalIdTests
    {
        static readonly ExternalId[] ExpectedOrder = new[]
                                    {
                                        ExternalId.Of("A", "1"),
                                        ExternalId.Of("A", "2"),
                                        ExternalId.Of("B", "1"),
                                        ExternalId.Of("B", "2"),
                                        ExternalId.Of("B", "2~3"),
                                    };

        [Fact]
        public void ComparableBehavesAsExpected()
        {
            for (int i = 0; i < ExpectedOrder.Length; i++)
            {
                var small = ExpectedOrder[i];

                Assert.Equal(0, small.CompareTo(small));
                
                for (int j = i + 1; j < ExpectedOrder.Length; j++)
                {
                    var big = ExpectedOrder[j];
                    Assert.InRange(small.CompareTo(big), int.MinValue, -1);
                    
                    Assert.InRange(big.CompareTo(small), 1, int.MaxValue);
                }
            }
        }

        [Fact]
        public void HashCodeBehavesAsExpected()
        {
            foreach (var identifier in ExpectedOrder)
            {
                Assert.Equal(1, ExpectedOrder.Where(e => e.GetHashCode() == identifier.GetHashCode()).Count());
                Assert.Equal(identifier.GetHashCode(), ExternalId.Of(identifier.Scheme, identifier.Value).GetHashCode());
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
            Assert.False(ExpectedOrder.First().Equals("SomeOtherType"));
        }

        [Fact]
        public void OperatorEqualsCodeBehavesAsExpected()
        {
            EqualsCodeBehavesAsExpected((a, b) => a == b);
        }

        private static void EqualsCodeBehavesAsExpected(Func<ExternalId, ExternalId, bool> equals)
        {
            foreach (var id in ExpectedOrder)
            {
                Assert.Equal(1, ExpectedOrder.Where(e => equals(e, id)).Count());
                Assert.True(equals(id, ExternalId.Of(id.Scheme, id.Value)));
            }
        }

        [Fact]
        public void ParseWorks()
        {
            foreach (var identifier in ExpectedOrder)
            {
                Assert.Equal(identifier, ExternalId.Parse(identifier.ToString()));
            }
        }

        [Theory]
        [InlineData("A")]
        public void WrongNumberOfSeparatorsFails(string uid)
        {
            Assert.Throws<ArgumentException>(() => ExternalId.Parse(uid));
        }
    }
}
