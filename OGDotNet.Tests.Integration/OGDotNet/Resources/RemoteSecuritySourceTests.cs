using System;
using System.Collections.Generic;
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
            Assert.Throws<ArgumentException>(() => remoteSecuritySource.GetSecurities(new IdentifierBundle(new HashSet<Identifier>())));
        }

        [Fact]
        public void CanDoEmptyBundleSingleQuery()
        {
            var remoteSecuritySource = Context.SecuritySource;
            Assert.Throws<ArgumentException>(() => remoteSecuritySource.GetSecurity(new IdentifierBundle(new HashSet<Identifier>())));
        }

        [Fact(Skip = "Known fault - [TODO add JIRA ID]")]
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
            var collection = remoteSecuritySource.GetSecurities(new IdentifierBundle(new HashSet<Identifier> {StupidIdentifier}));
            Assert.Empty(collection);
        }

        private static UniqueIdentifier StupidUid
        {
            get { return UniqueIdentifier.Of("xxx", "xxx"); }
        }

        private static Identifier StupidIdentifier
        {
            get { return new Identifier("xxx", "xxx"); }
        }
    }
}
