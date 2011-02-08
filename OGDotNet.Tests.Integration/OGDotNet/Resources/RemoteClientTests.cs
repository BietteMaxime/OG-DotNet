using OGDotNet.Tests.Integration.OGDotNet.Model.Context;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteClientTests
    {
        [Fact]
        public void CanCreateAndDispose()
        {
            using (RemoteEngineContextTests.GetContext().CreateUserClient())
            {
            }
        }
        
        //TODO heartbeat test
    }
}
