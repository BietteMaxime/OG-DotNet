// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteViewClientExtensions.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Mappedtypes.engine.View.listener;
using OGDotNet.Model.Resources;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public static class RemoteViewClientExtensions
    {
        public static IEnumerable<InMemoryViewComputationResultModel> GetResults(this RemoteViewClient client, string viewDefinitionName, IViewExecutionOptions executionOptions)
        {
            return client.GetResults(viewDefinitionName, executionOptions, false);
        }

        public static IEnumerable<InMemoryViewComputationResultModel> GetResults(this RemoteViewClient client, string viewDefinitionName, IViewExecutionOptions executionOptions, bool newBatchProcess)
        {
            return GetCycles(client, viewDefinitionName, executionOptions, newBatchProcess).Select(e => e.FullResult);
        }

        public static IEnumerable<CycleCompletedArgs> GetCycles(this RemoteViewClient client, string viewDefinitionName, IViewExecutionOptions executionOptions)
        {
            return GetCycles(client, viewDefinitionName, executionOptions, false);
        }

        public static IEnumerable<CycleCompletedArgs> GetCycles(this RemoteViewClient client, string viewDefinitionName, IViewExecutionOptions executionOptions, bool newBatchProcess)
        {
            //TODO handle errors

            using (var resultQueue = new BlockingCollection<CycleCompletedArgs>(new ConcurrentQueue<CycleCompletedArgs>()))
            {
                var resultListener = new EventViewResultListener();
                resultListener.CycleCompleted += (sender, e) => resultQueue.Add(e);

                client.SetResultListener(resultListener);

                client.AttachToViewProcess(viewDefinitionName, executionOptions, newBatchProcess);

                TimeSpan timeout = TimeSpan.FromMinutes(1);

                try
                {
                    while (true)
                    {
                        CycleCompletedArgs ret;
                        if (resultQueue.TryTake(out ret, (int)timeout.TotalMilliseconds))
                        {
                            yield return ret;
                        }
                        else
                        {
                            throw new TimeoutException(string.Format("No results received for {0} after {1} state {2} is completed {3}", viewDefinitionName, timeout, client.GetState(), client.IsCompleted));
                        }
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