using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.LiveData;

namespace OGDotNet.Model.Resources
{
    public class RemoteViewResource
    {
        private readonly RestTarget _rest;
        private readonly string _activeMqSpec;

        public RemoteViewResource(RestTarget rest, string activeMqSpec)
        {
            _rest = rest;
            _activeMqSpec = activeMqSpec;
        }

        public void Init()
        {
            _rest.Resolve("init").Post();
        }

        public IPortfolio Portfolio
        {
            get
            {
                var fudgeMsg = _rest.Resolve("portfolio").GetReponse();
                if (fudgeMsg == null)
                {
                    return null;
                }

                FudgeSerializer fudgeSerializer = FudgeConfig.GetFudgeSerializer();
                return fudgeSerializer.Deserialize<IPortfolio>(fudgeMsg);
            }
        }

        public ViewDefinition Definition
        {
            get
            {
                var fudgeMsg = _rest.Resolve("definition").GetReponse();
                return FudgeConfig.GetFudgeSerializer().Deserialize<ViewDefinition>(fudgeMsg);
            }
        }

        public ViewClientResource CreateClient()
        {
            
            var clientUri = _rest.Resolve("clients").Create(FudgeConfig.GetFudgeContext(), FudgeConfig.GetFudgeSerializer().SerializeToMsg(UserPrincipal.DefaultUser));

            return new ViewClientResource(clientUri, _activeMqSpec);
        }
    }
}