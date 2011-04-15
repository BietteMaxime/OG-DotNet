// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UniqueIdentifierTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;
using OGDotNet.Mappedtypes.Id;
using Xunit;

namespace OGDotNet.Tests.OGDotNet.Mappedtypes.Id
{
    public class UniqueIdentifierTests
    {
        static readonly UniqueIdentifier[] ExpectedOrder = new[]
                                    {
                                        UniqueIdentifier.Of("A", "1"),
                                        UniqueIdentifier.Of("A", "2"),
                                        UniqueIdentifier.Of("A", "2", "A"),
                                        UniqueIdentifier.Of("A", "2", "B"),
                                        UniqueIdentifier.Of("B", "1"),
                                        UniqueIdentifier.Of("B", "2"),
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
                Assert.Equal(uniqueIdentifier.GetHashCode(), UniqueIdentifier.Of(uniqueIdentifier.Scheme, uniqueIdentifier.Value, uniqueIdentifier.Version).GetHashCode());
            }
        }

        [Fact]
        public void EqualsCodeBehavesAsExpected()
        {
            foreach (var uniqueIdentifier in ExpectedOrder)
            {
                Assert.Equal(1, ExpectedOrder.Where(e => e.Equals(uniqueIdentifier)).Count());
                Assert.Equal(uniqueIdentifier, UniqueIdentifier.Of(uniqueIdentifier.Scheme, uniqueIdentifier.Value, uniqueIdentifier.Version));
            }
        }
    }
}
