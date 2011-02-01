using System;
using System.Collections.Generic;
using System.Threading;
using OGDotNet_Analytics.Mappedtypes.engine.View;

namespace OGDotNet_Analytics.Model.Resources
{
    /// <summary>
    /// DataViewClientResource
    /// </summary>
    public class ViewClientResource : DisposableBase  //TODO IObservable<ViewComputationResultModel>
    {
        private readonly string _activeMqSpec;
        private readonly RestTarget _rest;

        public ViewClientResource(Uri clientUri, string activeMqSpec)
        {
            _activeMqSpec = activeMqSpec;
            _rest = new RestTarget(clientUri);
        }

        public IEnumerable<ViewComputationResultModel> GetResults(CancellationToken token)
        {
            Start();
            while (!ResultAvailable)
            {
                if (token.IsCancellationRequested) yield break;
            }

            if (token.IsCancellationRequested) yield break;
            using (var deltaStream = StartDeltaStream())
            {//NOTE: by starting the delta stream first I believe I am ok to use this latest result

                if (token.IsCancellationRequested) yield break;
                ViewComputationResultModel results = LatestResult;
                
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
            _rest.GetSubMagic("start").GetReponse("POST");
        }

        private void Stop()//TODO make this stop the IEnumerables somehow
        {
            _rest.GetSubMagic("stop").GetReponse("POST");
        }

        public void Pause()
        {
            _rest.GetSubMagic("pause").GetReponse("POST");
        }

        private ClientResultStream<ViewComputationResultModel> StartResultStream()
        {
            var reponse = _rest.GetSubMagic("startJmsResultStream").GetFudgeReponse("POST");
            var queueName = reponse.GetValue<string>("value");
            var queueUri = new Uri(_activeMqSpec);
            return new ClientResultStream<ViewComputationResultModel>(queueUri, queueName, StopResultStream);
        }
        private void StopResultStream()
        {
            _rest.GetSubMagic("startJmsResultStream").GetReponse("POST");
        }

        private ClientResultStream<ViewComputationResultModel> StartDeltaStream()
        {
            var reponse = _rest.GetSubMagic("startJmsDeltaStream").GetFudgeReponse("POST");
            var queueName = reponse.GetValue<string>("value");
            var queueUri = new Uri(_activeMqSpec);
            return new ClientResultStream<ViewComputationResultModel>(queueUri, queueName, StopResultStream);
        }
        private void StopDeltaStream()
        {
            _rest.GetSubMagic("startJmsDeltaStream").GetReponse("POST");
        }

        private bool ResultAvailable
        {
            get {

                var reponse = _rest.GetSubMagic("resultAvailable").GetReponse();
                return 1 == (sbyte) (reponse.GetByName("value").Value);
            }
        }
        private ViewComputationResultModel LatestResult
        {
            get
            {
                var restMagic = _rest.GetSubMagic("latestResult").GetReponse();
                //ViewComputationResultModel
                var fudgeSerializer = FudgeConfig.GetFudgeSerializer();
                var wrapper = fudgeSerializer.Deserialize<Wrapper>(restMagic);
                return wrapper.LatestResult;
            }
        }
        private class Wrapper
        {
            public ViewComputationResultModel LatestResult { get; set; }
        }

        protected override void Dispose(bool disposing)
        {
            Stop();
        }
    }
}