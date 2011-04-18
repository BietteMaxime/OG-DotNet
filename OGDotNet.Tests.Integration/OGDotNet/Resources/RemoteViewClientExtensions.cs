// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteViewClientExtensions.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            //TODO handle errors

            using (var resultQueue = new BlockingCollection<InMemoryViewComputationResultModel>(new ConcurrentQueue<InMemoryViewComputationResultModel>()))
            {
                var resultListener = new EventViewResultListener();
                resultListener.CycleCompleted += (sender, e) => resultQueue.Add(e.FullResult);

                client.SetResultListener(resultListener);

                client.AttachToViewProcess(viewDefinitionName, executionOptions, newBatchProcess);
                try
                {
                    yield return resultQueue.Take();
                }
                finally
                {
                    client.RemoveResultListener();
                }
            }
        }
    }
}