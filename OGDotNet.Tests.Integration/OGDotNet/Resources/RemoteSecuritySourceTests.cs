using System;
using System.Collections.Generic;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Model.Context;
using OGDotNet.Tests.Integration.OGDotNet.Model.Context;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteSecuritySourceTests
    {
        private readonly RemoteEngineContext _context;

        public RemoteSecuritySourceTests()
        {
            _context = RemoteEngineContextTests.GetContext();
        }
        [Fact]
        public void CanCreate()
        {
            var remoteSecuritySource = _context.SecuritySource;
            Assert.NotNull(remoteSecuritySource);
        }

        [Fact]
        public void CanDoEmptyBundleMultiQuery()
        {
            var remoteSecuritySource = _context.SecuritySource;
            Assert.Throws<ArgumentException>(() => remoteSecuritySource.GetSecurities(new IdentifierBundle(new HashSet<Identifier>())));
        }

        [Fact]
        public void CanDoEmptyBundleSingleQuery()
        {
            var remoteSecuritySource = _context.SecuritySource;
            Assert.Throws<ArgumentException>(() => remoteSecuritySource.GetSecurity(new IdentifierBundle(new HashSet<Identifier>())));
        }

        [Fact(Skip = "Known fault - [TODO add JIRA ID")]
        public void CanDoMissingUidQuery()
        {
            var remoteSecuritySource = _context.SecuritySource;
            var security = remoteSecuritySource.GetSecurity(StupidUid);
            Assert.Null(security);
        }

        [Fact]
        public void CanDoEmptyMissingBundleQuery()
        {
            var remoteSecuritySource = _context.SecuritySource;
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
