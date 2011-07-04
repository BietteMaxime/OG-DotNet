//-----------------------------------------------------------------------
// <copyright file="IdentifierTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Linq;
using OGDotNet.Mappedtypes.Id;
using Xunit;

namespace OGDotNet.Tests.OGDotNet.Mappedtypes.Id
{
    public class IdentifierTests
    {
        static readonly Identifier[] ExpectedOrder = new[]
                                    {
                                        Identifier.Of("A", "1"),
                                        Identifier.Of("A", "2"),
                                        Identifier.Of("B", "1"),
                                        Identifier.Of("B", "2"),
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
                Assert.Equal(identifier.GetHashCode(), Identifier.Of(identifier.Scheme, identifier.Value).GetHashCode());
            }
        }

        [Fact]
        public void EqualsCodeBehavesAsExpected()
        {
            foreach (var identifier in ExpectedOrder)
            {
                Assert.Equal(1, ExpectedOrder.Where(e => e.Equals(identifier)).Count());
                Assert.Equal(identifier, Identifier.Of(identifier.Scheme, identifier.Value));
            }
        }

        [Fact]
        public void ParseWorks()
        {
            foreach (var identifier in ExpectedOrder)
            {
                Assert.Equal(identifier, Identifier.Parse(identifier.ToString()));
            }
        }
    }
}
