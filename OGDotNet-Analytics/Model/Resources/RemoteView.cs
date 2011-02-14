using System;
using System.Threading;
using Apache.NMS;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.LiveData;

namespace OGDotNet.Model.Resources
{
    public class RemoteView
    {
        private readonly RestTarget _rest;
        private readonly string _activeMqSpec;
        private readonly string _name;
        private readonly MQTemplate _mqTemplate;

        public RemoteView(RestTarget rest, string activeMqSpec, string name)
        {
            _rest = rest;
            _activeMqSpec = activeMqSpec;
            _name = name;
            _mqTemplate = new MQTemplate(_activeMqSpec);
        }

        public string Name
        {
            get { return _name; }
        }

        public void Init(CancellationToken cancellationToken = new CancellationToken())
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
                            IMessage message= null;
                            while (message == null)//TODO make this cancellable in a more sane way
                            {
                                message = consumer.Receive(TimeSpan.FromMilliseconds(1000));
                                cancellationToken.ThrowIfCancellationRequested();
                            }


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