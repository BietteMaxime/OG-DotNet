using System.Collections.Generic;
using System.Threading;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Mappedtypes.engine.View.listener;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    public static class RemoteViewClientExtensions
    {
        public static IEnumerable<InMemoryViewComputationResultModel> GetResults(this RemoteViewClient client, string viewDefinitionName, IViewExecutionOptions executionOptions, CancellationToken token = default(CancellationToken))
        {
            return client.GetResults(viewDefinitionName, executionOptions, false, token);
        }

        public static IEnumerable<InMemoryViewComputationResultModel> GetResults(this RemoteViewClient client, string viewDefinitionName, IViewExecutionOptions executionOptions, bool newBatchProcess, CancellationToken token = default(CancellationToken))
        {
            //TODO handle errors
            using (var resultQueue = new BlockingQueueWithCancellation<InMemoryViewComputationResultModel>(token))
            {
                var resultListener = new EventViewResultListener();
                resultListener.CycleCompleted += (sender, e) => resultQueue.Enqueue(e.FullResult);

                client.SetResultListener(resultListener);

                client.AttachToViewProcess(viewDefinitionName, executionOptions, newBatchProcess);
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        yield return resultQueue.Dequeue();
                    }
                }
                finally
                {
                    client.RemoveResultListener();
                }
            }
        }
    }
}