using OGDotNet.Model.Context;
using OGDotNet.Tests.Integration.OGDotNet.Model.Context;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class TestWithContextBase
    {
        protected readonly RemoteEngineContext Context;

        public TestWithContextBase()
        {
            Context = GetContext();
        }
        protected static RemoteEngineContext GetContext()
        {
            return RemoteEngineContextTests.GetContext();
        }
    }
}