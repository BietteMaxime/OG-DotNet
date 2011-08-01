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
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Mappedtypes.Engine.View.Execution;
using OGDotNet.Mappedtypes.Engine.View.listener;
using OGDotNet.Model.Resources;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public static class RemoteViewClientExtensions
    {
        public static IEnumerable<IViewComputationResultModel> GetResults(this RemoteViewClient client, string viewDefinitionName, IViewExecutionOptions executionOptions)
        {
            return client.GetResults(viewDefinitionName, executionOptions, false);
        }

        public static IEnumerable<IViewComputationResultModel> GetResults(this RemoteViewClient client, string viewDefinitionName, IViewExecutionOptions executionOptions, bool newBatchProcess)
        {
            return GetCycles(client, viewDefinitionName, executionOptions, newBatchProcess).Select(e => e.FullResult);
        }

        public static IEnumerable<CycleCompletedArgs> GetCycles(this RemoteViewClient client, string viewDefinitionName, IViewExecutionOptions executionOptions)
        {
            return GetCycles(client, viewDefinitionName, executionOptions, false);
        }

        public static IEnumerable<CycleCompletedArgs> GetCycles(this RemoteViewClient client, string viewDefinitionName, IViewExecutionOptions executionOptions, bool newBatchProcess)
        {
            using (var resultQueue = new BlockingCollection<object>(new ConcurrentQueue<object>()))
            using (var otherQueue = new BlockingCollection<object>(new ConcurrentQueue<object>()))
            {
                var resultListener = new EventViewResultListener();
                resultListener.CycleCompleted += (sender, e) => resultQueue.Add(e);

                resultListener.CycleExecutionFailed += (s, e) => otherQueue.Add(e);
                resultListener.ProcessTerminated += (s, e) => otherQueue.Add(e);
                resultListener.ViewDefinitionCompilationFailed += (s, e) => otherQueue.Add(e);

                client.SetResultListener(resultListener);

                client.AttachToViewProcess(viewDefinitionName, executionOptions, newBatchProcess);

                TimeSpan timeout = TimeSpan.FromMinutes(1);

                try
                {
                    while (true)
                    {
                        object next;
                        var index = BlockingCollection<object>.TryTakeFromAny(new[] { resultQueue, otherQueue }, out next, timeout);
                        if (index == 0)
                        {
                            yield return (CycleCompletedArgs)next;
                        }
                        else
                        {
                            var detailMessage = string.Format("for {0} after {1}\n state {2} is completed {3}", viewDefinitionName, timeout, client.GetState(), client.IsCompleted);
                            switch (index)
                            {
                                case 0:
                                    throw new ArgumentException("index");
                                case 1:
                                    throw new Exception(string.Format("Error occured whilst getting results {0}\n{1}", next, detailMessage));
                                default:

                                    throw new TimeoutException("No results received " + detailMessage);
                            }
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