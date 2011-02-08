using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OGDotNet.Model.Context;
using OGDotNet.Tests.Integration.Properties;
using Xunit;

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
