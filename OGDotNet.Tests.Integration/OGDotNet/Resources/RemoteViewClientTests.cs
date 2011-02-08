using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using OGDotNet.Model.Resources;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using Xunit;
using Xunit.Extensions;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteViewClientTests: TestWithContextBase
    {
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
            if (view.Name.Contains("10K"))
                return;//Skip this, it's sloooow

            view.Init();
            using (var remoteViewClient = view.CreateClient())
            {
                remoteViewClient.Start();

                var cts = new CancellationTokenSource();
                var resultsEnum = remoteViewClient.GetResults(cts.Token);
                using (var enumerator = resultsEnum.GetEnumerator())
                {
                    enumerator.MoveNext();
                    Assert.NotNull(enumerator.Current);
                }
            }
        }

        public static IEnumerable<RemoteView> Views
        {
            get
            {
                var remoteEngineContext = GetContext();
                return remoteEngineContext.ViewProcessor.ViewNames.Select(n => remoteEngineContext.ViewProcessor.GetView(n));
            }
        }

    }
}
