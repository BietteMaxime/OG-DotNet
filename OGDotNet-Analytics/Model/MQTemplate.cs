using System;
using Apache.NMS;

namespace OGDotNet.Model
{
    public class MQTemplate
    {
        private readonly string _activeMqSpec;
        private readonly NMSConnectionFactory _factory;

        public MQTemplate(string activeMqSpec)
        {
            _activeMqSpec = activeMqSpec;
            var oldSkooluri = new Uri(_activeMqSpec).LocalPath.Replace("(", "").Replace(")", "");
            _factory = new NMSConnectionFactory(oldSkooluri);
        }

        public IConnection CreateConnection()
        {
            return _factory.CreateConnection();
        }

        public void Do(Action<ISession> action)
        {


            using (var connection = CreateConnection())
            using (var session = connection.CreateSession())
            {
                connection.Start();
                action(session);
            }
        }

       
    }
}