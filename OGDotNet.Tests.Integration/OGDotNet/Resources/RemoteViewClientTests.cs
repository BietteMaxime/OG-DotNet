using System;
using System.Threading;
using OGDotNet.Model.Resources;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using Xunit;
using Xunit.Extensions;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteViewClientTests: ViewTestsBase
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
            GetSomeResults(view,1);
        }

        [Theory]
        [TypedPropertyData("Views")]
        public void CanGetManyResults(RemoteView view)
        {
            GetSomeResults(view, 3);
        }

        private static void GetSomeResults(RemoteView view, int resultsToGet)
        {
            view.Init();
            using (var remoteViewClient = view.CreateClient())
            {
                var cts = new CancellationTokenSource();
                var resultsEnum = remoteViewClient.GetResults(cts.Token);

                int i = 0;
                foreach (var viewComputationResultModel in resultsEnum)
                {
                    Assert.NotNull(viewComputationResultModel);
                    
                    i++;

                    if (i >= resultsToGet)
                        break;
                }

            }
        }


        [Theory]
        [TypedPropertyData("Views")]
        public void CanPauseAndRestart(RemoteView view)
        {
            const int forbiddenAfterPause = 3;

            var timeout = TimeSpan.FromMilliseconds(10000 * forbiddenAfterPause);

            view.Init();
            using (var remoteViewClient = view.CreateClient())
            {
                var cts = new CancellationTokenSource();
                var resultsEnum = remoteViewClient.GetResults(cts.Token);

                using (var enumerator = resultsEnum.GetEnumerator())
                {
                    enumerator.MoveNext();
                    Assert.NotNull(enumerator.Current);

                    remoteViewClient.Pause();

                    var mre = new ManualResetEvent(false);
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        for (int i = 0; i < forbiddenAfterPause; i++)
                        {
                            enumerator.MoveNext();
                            Assert.NotNull(enumerator.Current);
                        }
                        mre.Set();
                    });

                    bool gotResults = mre.WaitOne(timeout);
                    Assert.False(gotResults, string.Format("I got {0} results for view {1} within {2} after pausing", forbiddenAfterPause, view.Name, timeout));

                    remoteViewClient.Start();
                    gotResults = mre.WaitOne(timeout);
                    Assert.True(gotResults, string.Format("I didn't {0} results for view {1} within {2} after starting", forbiddenAfterPause, view.Name, timeout));
                }

            }
        }
    }
}
