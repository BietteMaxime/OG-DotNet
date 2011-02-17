using OGDotNet.Model.Context;
using OGDotNet.Tests.Integration.OGDotNet.Model.Context;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class TestWithContextBase
    {
        private RemoteEngineContext _context;
        protected RemoteEngineContext Context { 
            get
            {
                _context = _context ?? GetContext();
                return _context ?? GetContext();
            }
        }

        protected static RemoteEngineContext GetContext()
        {
            return RemoteEngineContextTests.GetContext();
        }
    }
}