using OGDotNet.Tests.Integration.OGDotNet.Model.Context;
using FactAttribute = OGDotNet.Tests.Integration.Xunit.Extensions.FactAttribute;

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
