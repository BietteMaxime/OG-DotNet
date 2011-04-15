//-----------------------------------------------------------------------
// <copyright file="RemoteViewClient.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Mappedtypes.financial.view.rest;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.util.PublicAPI;
using OGDotNet.Mappedtypes.util.timeseries.fast;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    /// <summary>
    /// See DataViewClientResource on the java side
    /// </summary>
    public class RemoteViewClient : DisposableBase  //TODO IObservable<ViewComputationResultModel>
    {
        private readonly OpenGammaFudgeContext _fudgeContext;
        private readonly MQTemplate _mqTemplate;
        private readonly RestTarget _rest;

        public RemoteViewClient(OpenGammaFudgeContext fudgeContext, RestTarget clientUri, MQTemplate mqTemplate)
        {
            _fudgeContext = fudgeContext;
            _mqTemplate = mqTemplate;
            _rest = clientUri;
        }

        public IEnumerable<InMemoryViewComputationResultModel> GetResults(CancellationToken token)
        {
            if (token.IsCancellationRequested) yield break;
            using (var deltaStream = StartResultStream())
            {//NOTE: by starting the delta stream first I believe I am ok to use this latest result

                while (!IsResultAvailable)
                {//TODO this is unnecesary
                    if (token.IsCancellationRequested) yield break;
                }

                if (token.IsCancellationRequested) yield break;
                var results = GetLatestResult();
                
                while (!token.IsCancellationRequested)
                {
                    yield return results;

                    InMemoryViewComputationResultModel delta;
                    try
                    {
                        delta = deltaStream.GetNext(token);
                    }
                    catch (OperationCanceledException)
                    {
                        yield break;
                    }
                    results = results.ApplyDelta(delta);
                }
            }
        }

        public void Pause()
        {
            _rest.Resolve("pause").Post();
        }

        public void Resume()
        {
            _rest.Resolve("resume").Post();
        }

        public void Shutdown()
        {
            _rest.Resolve("shutdown").Post();
        }

        private ClientResultStream StartResultStream()
        {
            var reponse = _rest.Resolve("startJmsResultStream").Post();
            return new ClientResultStream(_fudgeContext, _mqTemplate, reponse.GetValue<string>("value"), StopResultStream);
        }
        private void StopResultStream()
        {
            _rest.Resolve("endJmsResultStream").Post();
        }

        public void SetUpdatePeriod(long periodMillis)
        {
            _rest.Resolve("updatePeriod").Post<object>(periodMillis, "updatePeriod");
        }

        public bool IsAttached
        {
            get
            {
                var reponse = _rest.Resolve("isAttached").GetFudge();
                return 1 == (sbyte)reponse.GetByName("value").Value;
            }
        }

        public void AttachToViewProcess(string viewDefinitionName, IViewExecutionOptions executionOptions)
        {
            AttachToViewProcess(viewDefinitionName, executionOptions, false);
        }

        public void AttachToViewProcess(string viewDefinitionName, IViewExecutionOptions executionOptions, bool newBatchProcess)
        {
            AttachToViewProcessRequest request = new AttachToViewProcessRequest(viewDefinitionName, executionOptions, newBatchProcess);
            var reponse = _rest.Resolve("attachSearch").Post(request);
        }
        public void DetachFromViewProcess()
        {
            _rest.Resolve("detach").Post();
        }


        public RemoteLiveDataInjector LiveDataOverrideInjector
        {
            get
            {
                return new RemoteLiveDataInjector(_rest.Resolve("overrides"));
            }
        }

        public bool IsResultAvailable//TODO use batch
        {
            get
            {
                var reponse = _rest.Resolve("resultAvailable").GetFudge();
                return 1 == (sbyte)reponse.GetByName("value").Value;
            }
        }

        public bool IsCompleted
        {
            get
            {
                var reponse = _rest.Resolve("completed").GetFudge();
                return 1 == (sbyte)reponse.GetByName("value").Value;
            }
        }

        public InMemoryViewComputationResultModel GetLatestResult()
        {
            return _rest.Resolve("latestResult").Get<InMemoryViewComputationResultModel>();
        }

        public UniqueIdentifier GetUniqueId()
        {
            var restTarget = _rest.Resolve("id");
            return restTarget.Get<UniqueIdentifier>();
        }
        public ViewClientState GetState()
        {
            var target = _rest.Resolve("state");
            var msg = target.GetFudge();

            return EnumBuilder<ViewClientState>.Parse((string) msg.GetByOrdinal(1).Value);
        }

        protected override void Dispose(bool disposing)
        {
            Shutdown();
        }
    }
}