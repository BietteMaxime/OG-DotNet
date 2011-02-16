using System;
using OGDotNet.Model.Context;
using OGDotNet.Tests.Integration.Properties;
using Xunit;
using FactAttribute = OGDotNet.Tests.Integration.Xunit.Extensions.FactAttribute;

namespace OGDotNet.Tests.Integration.OGDotNet.Model.Context
{
    public class RemoteEngineContextTests
    {
        [Fact]
        public void CanCreate()
        {
            GetContext();
        }

        [Fact]
        public void RootUriAsExpected()
        {
            Assert.Equal(new Uri(Settings.Default.ServiceUri), GetContext().RootUri);
        }

        internal static RemoteEngineContext GetContext()
        {
            var remoteEngineContextFactory = RemoteEngineContextFactoryTests.GetContextFactory();
            return remoteEngineContextFactory.CreateRemoteEngineContext();
        }
    }
}
