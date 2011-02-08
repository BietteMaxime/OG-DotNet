using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using OGDotNet.Model.Resources;
using OGDotNet.Tests.Integration.Properties;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using Xunit;
using Xunit.Extensions;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteViewClientTests: TestWithContextBase
    {

        private static readonly HashSet<string> BannedViews = new HashSet<string>
                                                                  {
                                                                      "10K Swap Test View",//Slow
                                                                      "TestDefinition"//Broken
                                                                  };

        [Theory]
        [TypedPropertyData("Views")]
        public void CanGet(RemoteView view)
        {
            view.Init();
            using (var remoteViewClient = view.CreateClient())
            {
                Assert.NotNull(remoteViewClient);
            }
        }

        [Theory]
        [TypedPropertyData("Views")]
        public void CanStartAndGetAResult(RemoteView view)
        {
            view.Init();
            using (var remoteViewClient = view.CreateClient())
            {
                var cts = new CancellationTokenSource();
                var resultsEnum = remoteViewClient.GetResults(cts.Token);
                using (var enumerator = resultsEnum.GetEnumerator())
                {
                    enumerator.MoveNext();
                    Assert.NotNull(enumerator.Current);
                }
            }
        }

        //TODO pause tests

        public static IEnumerable<RemoteView> Views
        {
            get
            {
                var remoteEngineContext = GetContext();
                return remoteEngineContext.ViewProcessor.ViewNames.Where(n => !BannedViews.Contains(n)).Select(n => remoteEngineContext.ViewProcessor.GetView(n));
            }
        }

    }
}
