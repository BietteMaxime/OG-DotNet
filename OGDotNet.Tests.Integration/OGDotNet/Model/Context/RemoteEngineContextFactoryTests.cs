using OGDotNet.Model.Context;
using OGDotNet.Tests.Integration.Properties;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Model.Context
{
    public class RemoteEngineContextFactoryTests
    {
        [Fact]
        public void CanCreateRemoteEngineContext()
        {
            GetContextFactory();
        }

        internal static RemoteEngineContextFactory GetContextFactory()
        {
            return new RemoteEngineContextFactory(Settings.Default.ServiceUri, Settings.Default.ConfigId);
        }
    }
}
