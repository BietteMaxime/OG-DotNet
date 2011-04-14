//-----------------------------------------------------------------------
// <copyright file="RemoteView.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using Apache.NMS;
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.LiveData;

namespace OGDotNet.Model.Resources
{
    public class RemoteView
    {
        private readonly OpenGammaFudgeContext _fudgeContext;
        private readonly RestTarget _rest;
        private readonly string _activeMqSpec;
        private readonly string _name;
        private readonly MQTemplate _mqTemplate;

        public RemoteView(OpenGammaFudgeContext fudgeContext, RestTarget rest, string activeMqSpec, string name)
        {
            _fudgeContext = fudgeContext;
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
            //This post responds via JMS
            //See the java comments for explanation
            IMessage message = CallResponse( cancellationToken, topicName => _rest.Resolve("init").Post(topicName));

            var value = (bool)message.Properties["init"];
            if (!value)
            {
                throw new ArgumentException(string.Format("View {0} failed to initialize", Name));
            }
        }

        private IMessage CallResponse(CancellationToken token, Action<string> call)
        {
            IMessage ret = null;
            _mqTemplate.Do(delegate(ISession session)
            {
                var temporaryTopic = session.CreateTemporaryTopic(); // TODO we shouldn't be creating temporary topics all over the place
                try
                {
                    using (var consumer = session.CreateConsumer(temporaryTopic))
                    using (var mre = new ManualResetEvent(false))
                    {
                        IMessage message = null;
                        consumer.Listener += delegate(IMessage m)
                                                 {
                                                     message = m;
                                                     mre.Set();
                                                 };

                        token.ThrowIfCancellationRequested();
                        call(temporaryTopic.TopicName);


                        var waitHandles = new[] { mre, token.WaitHandle };
                        while (WaitHandle.WaitAny(waitHandles) != 0)
                        {
                            token.ThrowIfCancellationRequested();
                        }
                        ret = message;
                    }
                }
                finally
                {
                    temporaryTopic.Delete();
                }
            }
           );
            return ret;
        }

        public IPortfolio Portfolio
        {
            get
            {
                return _rest.Resolve("portfolio").Get<IPortfolio>();
            }
        }

        public ViewDefinition Definition
        {
            get
            {
                return _rest.Resolve("definition").Get<ViewDefinition>();
            }
        }

        public RemoteLiveDataInjector LiveDataOverrideInjector
        {
            get
            {
                return new RemoteLiveDataInjector(_rest.Resolve("liveDataOverrideInjector"));
            }
        }

        public RemoteViewClient CreateClient()
        {
            var clientUri = _rest.Resolve("clients").Create(UserPrincipal.DefaultUser);

            return new RemoteViewClient(_fudgeContext, clientUri, _mqTemplate);
        }

        public void AssertAccessToLiveDataRequirements(UserPrincipal user)
        {
            _rest.Resolve("hasAccessToLiveData", Tuple.Create("userIp", user.IpAddress), Tuple.Create("userName", user.UserName)).Post();
        }


        public IEnumerable<ValueRequirement> GetRequiredLiveData()
        {
            var target = _rest.Resolve("requiredLiveData");
            return new List<ValueRequirement>(target.Get<ValueRequirement[]>());
        }

        public HashSet<string> GetAllSecurityTypes()
        {
            var target = _rest.Resolve("allSecurityTypes");
            return new HashSet<string>(target.Get<string[]>());
        }

        public bool IsLiveComputationRunning()
        {
            var target = _rest.Resolve("liveComputationRunning");
            var reponse = target.GetFudge();
            return 1 == (sbyte)reponse.GetByName("value").Value;
        }



        public override string ToString()
        {
            return string.Format("[View {0}]", _name);
        }
    }
}