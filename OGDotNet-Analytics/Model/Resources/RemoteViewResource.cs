using System;
using Apache.NMS;
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
        private readonly MQTemplate _mqTemplate;

        public RemoteViewResource(RestTarget rest, string activeMqSpec)
        {
            _rest = rest;
            _activeMqSpec = activeMqSpec;
            _mqTemplate = new MQTemplate(_activeMqSpec);
        }

        public void Init()
        {
            _mqTemplate.Do(delegate(ISession session)
                {
                    var temporaryTopic = session.CreateTemporaryTopic();
                    try
                    {
                        using (var consumer = session.CreateConsumer(temporaryTopic))
                        {
                            //This post responds via JMS
                            //See the java comments for explanation
                            _rest.Resolve("init").Post(temporaryTopic.TopicName);
                            IMessage message = consumer.Receive();
                            
                            bool value = (bool) message.Properties["init"];

                            if (!value)
                            {
                                throw new ArgumentException("View failed to initialize");
                            }
                        }
                    }
                    finally
                    {
                        temporaryTopic.Delete();//Oh how I wish this was a dipose call
                    }
                }
            );
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

        public RemoteViewClient CreateClient()
        {
            
            var clientUri = _rest.Resolve("clients").Create(FudgeConfig.GetFudgeContext(), FudgeConfig.GetFudgeSerializer().SerializeToMsg(UserPrincipal.DefaultUser));

            return new RemoteViewClient(clientUri, _mqTemplate);
        }
    }
}