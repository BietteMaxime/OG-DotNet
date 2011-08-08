//-----------------------------------------------------------------------
// <copyright file="RemoteSecuritySourceTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using OGDotNet.Mappedtypes.Id;
using Xunit;
using FactAttribute = OGDotNet.Tests.Integration.Xunit.Extensions.FactAttribute;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteSecuritySourceTests : TestWithContextBase
    {
        [Fact]
        public void CanCreate()
        {
            var remoteSecuritySource = Context.SecuritySource;
            Assert.NotNull(remoteSecuritySource);
        }

        [Fact]
        public void CanDoEmptyBundleMultiQuery()
        {
            var remoteSecuritySource = Context.SecuritySource;
            Assert.Throws<ArgumentException>(() => remoteSecuritySource.GetSecurities(new ExternalIdBundle()));
        }

        [Fact]
        public void CanDoEmptyBundleSingleQuery()
        {
            var remoteSecuritySource = Context.SecuritySource;
            Assert.Throws<ArgumentException>(() => remoteSecuritySource.GetSecurity(new ExternalIdBundle()));
        }

        [Fact]
        public void CanDoMissingUidQuery()
        {
            var remoteSecuritySource = Context.SecuritySource;
            var security = remoteSecuritySource.GetSecurity(StupidUid);
            Assert.Null(security);
        }

        [Fact]
        public void CanDoEmptyMissingBundleQuery()
        {
            var remoteSecuritySource = Context.SecuritySource;
            var collection = remoteSecuritySource.GetSecurities(new ExternalIdBundle(StupidIdentifier));
            Assert.Empty(collection);
        }

        [Fact]
        public void CanGetBondsByIssuerName()
        {
            var remoteSecuritySource = Context.SecuritySource;
            var collection = remoteSecuritySource.GetBondsWithIssuerName("US TREASURY N/B");
            Assert.NotEmpty(collection);
            foreach (var security in collection)
            {
                ValueAssertions.AssertSensibleValue(security);
                Assert.Equal("BOND", security.SecurityType);
            }
        }

        [Fact]
        public void CanGetNoBondsByIssuerName()
        {
            var remoteSecuritySource = Context.SecuritySource;
            var collection = remoteSecuritySource.GetBondsWithIssuerName(Guid.NewGuid().ToString());
            Assert.Empty(collection);
        }

        private static UniqueId StupidUid
        {
            get { return UniqueId.Of("xxx", "xxx"); }
        }

        private static ExternalId StupidIdentifier
        {
            get { return new ExternalId("xxx", "xxx"); }
        }
    }
}
