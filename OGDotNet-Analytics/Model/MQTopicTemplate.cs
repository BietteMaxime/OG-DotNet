//-----------------------------------------------------------------------
// <copyright file="MQTopicTemplate.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
namespace OGDotNet.Model
{
    public class MQTopicTemplate
    {
        private readonly MQTemplate _template;
        private readonly string _topicName;

        public MQTopicTemplate(MQTemplate template, string topicName)
        {
            _template = template;
            _topicName = topicName;
        }

        public MQTemplate Template
        {
            get { return _template; }
        }

        public string TopicName
        {
            get { return _topicName; }
        }
    }
}