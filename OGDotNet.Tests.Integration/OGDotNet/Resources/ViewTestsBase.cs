using System.Collections.Generic;
using System.Linq;
using OGDotNet.Model.Resources;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class ViewTestsBase : TestWithContextBase
    {
        private static readonly HashSet<string> BannedViews = new HashSet<string>
                                                                  {
                                                                      "10K Swap Test View",//Slow
                                                                      "TestDefinition"//Broken
                                                                  };

        public static IEnumerable<string> ViewNames
        {
            get
            {
                var remoteEngineContext = GetContext();
                return remoteEngineContext.ViewProcessor.ViewNames.Where(n => !BannedViews.Contains(n));
            }
        }

        public static IEnumerable<RemoteView> Views
        {
            get
            {
                var remoteEngineContext = GetContext();
                return ViewNames.Where(n => !BannedViews.Contains(n)).Select(n => remoteEngineContext.ViewProcessor.GetView(n));
            }
        }
    }
}