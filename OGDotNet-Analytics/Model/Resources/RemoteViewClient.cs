using System;
using System.Collections.Generic;
using System.Threading;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    /// <summary>
    /// DataViewClientResource
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

        public IEnumerable<ViewComputationResultModel> GetResults(CancellationToken token)
        {
            Start();

            if (token.IsCancellationRequested) yield break;
            using (var deltaStream = StartDeltaStream())
            {//NOTE: by starting the delta stream first I believe I am ok to use this latest result

                while (!ResultAvailable)
                {
                    if (token.IsCancellationRequested) yield break;
                }

                if (token.IsCancellationRequested) yield break;
                ViewComputationResultModel results = GetLatestResult();
                
                while (!token.IsCancellationRequested)
                {
                    yield return results;

                    ViewComputationResultModel delta;
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

        public void Start()
        {
            _rest.Resolve("start").Post();
        }

        private void Stop()//TODO make this stop the IEnumerables somehow
        {
            _rest.Resolve("stop").Post();
        }

        public void Pause()
        {
            _rest.Resolve("pause").Post();
        }

        private ClientResultStream<ViewComputationResultModel> StartResultStream()
        {
            var reponse = _rest.Resolve("startJmsResultStream").Post();
            return new ClientResultStream<ViewComputationResultModel>(_fudgeContext, _mqTemplate, reponse.GetValue<string>("value"), StopResultStream);
        }
        private void StopResultStream()
        {
            _rest.Resolve("endJmsResultStream").Post();
        }

        private ClientResultStream<ViewComputationResultModel> StartDeltaStream()
        {
            var reponse = _rest.Resolve("startJmsDeltaStream").Post();
            return new ClientResultStream<ViewComputationResultModel>(_fudgeContext, _mqTemplate, reponse.GetValue<string>("value"), StopDeltaStream);
        }
        private void StopDeltaStream()
        {
            _rest.Resolve("endJmsDeltaStream").Post();
        }

        public bool ResultAvailable
        {
            get {

                var reponse = _rest.Resolve("resultAvailable").GetFudge();
                return 1 == (sbyte) (reponse.GetByName("value").Value);
            }
        }


        public ViewComputationResultModel RunOneCycle(long valuationTime)
        {
            return _rest.Resolve("runOneCycle").Post<ViewComputationResultModel>(valuationTime, "runOneCycle");
        }

        public ViewComputationResultModel GetLatestResult()
        {
            return _rest.Resolve("latestResult").Get<ViewComputationResultModel>("latestResult");
        }

        public UniqueIdentifier GetUniqueId()
        {
            var restTarget = _rest.Resolve("uniqueIdentifier");
            return restTarget.Get<UniqueIdentifier>();
        }

        protected override void Dispose(bool disposing)
        {
            Stop();
        }
    }
}