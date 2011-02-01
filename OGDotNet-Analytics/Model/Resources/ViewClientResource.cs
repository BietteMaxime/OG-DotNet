using System;
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

        public void Start()
        {
            _rest.GetSubMagic("start").GetReponse("POST");
        }
        public void Stop()
        {
            _rest.GetSubMagic("stop").GetReponse("POST");
        }

        public void Pause()
        {
            _rest.GetSubMagic("pause").GetReponse("POST");
        }

        public ClientResultStream<ViewComputationResultModel> StartResultStream()
        {
            var reponse = _rest.GetSubMagic("startJmsResultStream").GetFudgeReponse("POST");
            var queueName = reponse.GetValue<string>("value");
            var queueUri = new Uri(_activeMqSpec);
            return new ClientResultStream<ViewComputationResultModel>(queueUri, queueName, StopResultStream);
        }
        public void StopResultStream()
        {
            _rest.GetSubMagic("startJmsResultStream").GetReponse("POST");
        }

        public ClientResultStream<ViewComputationResultModel> StartDeltaStream()
        {
            var reponse = _rest.GetSubMagic("startJmsDeltaStream").GetFudgeReponse("POST");
            var queueName = reponse.GetValue<string>("value");
            var queueUri = new Uri(_activeMqSpec);
            return new ClientResultStream<ViewComputationResultModel>(queueUri, queueName, StopResultStream);
        }
        public void StopDeltaStream()
        {
            _rest.GetSubMagic("startJmsDeltaStream").GetReponse("POST");
        }

        public bool ResultAvailable
        {
            get {

                var reponse = _rest.GetSubMagic("resultAvailable").GetReponse();
                return 1 == (sbyte) (reponse.GetByName("value").Value);
            }
        }
        public ViewComputationResultModel LatestResult
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
        public class Wrapper
        {
            public ViewComputationResultModel LatestResult { get; set; }
        }

        protected override void Dispose(bool disposing)
        {
            Stop();
        }
    }
}