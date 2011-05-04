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
            using (var resultQueue = new BlockingCollection<CycleCompletedArgs>(new ConcurrentQueue<CycleCompletedArgs>()))
            using (var otherQueue = new BlockingCollection<object>(new ConcurrentQueue<object>()))
            {
                var resultListener = new EventViewResultListener();
                resultListener.CycleCompleted += (sender, e) => resultQueue.Add(e);

                resultListener.CycleExecutionFailed += (s, e) => otherQueue.Add(e);
                resultListener.ProcessCompleted += (s, e) => otherQueue.Add(e);
                resultListener.ProcessTerminated += (s, e) => otherQueue.Add(e);
                resultListener.ViewDefinitionCompilationFailed += (s, e) => otherQueue.Add(e);
                resultListener.ViewDefinitionCompiled += (s, e) => otherQueue.Add(e);

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
                            throw new TimeoutException(string.Format("No results received for {0} after {1}\n{2}\n state {3} is completed {4}", viewDefinitionName, timeout, 
                                string.Join(",", otherQueue.Select(o=>o.ToString())),
                                client.GetState(), client.IsCompleted));
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