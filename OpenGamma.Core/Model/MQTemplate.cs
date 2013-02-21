// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MQTemplate.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Apache.NMS;

namespace OpenGamma.Model
{
    public class MQTemplate
    {
        private readonly string _activeMqSpec;
        private readonly NMSConnectionFactory _factory;

        public MQTemplate(string activeMqSpec)
        {
            _activeMqSpec = activeMqSpec;
            var oldSkooluri = new Uri(_activeMqSpec).LocalPath.Replace("(", string.Empty).Replace(")", string.Empty);
            _factory = new NMSConnectionFactory(oldSkooluri);
        }

        public IConnection CreateConnection()
        {
            return _factory.CreateConnection();
        }

        public string ActiveMqSpec
        {
            get { return _activeMqSpec; }
        }
    }
}