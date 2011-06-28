//-----------------------------------------------------------------------
// <copyright file="FudgeMessageDecoder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.IO;
using Apache.NMS;
using Apache.NMS.ActiveMQ.Commands;

namespace OGDotNet.Model.Resources
{
    public class FudgeMessageDecoder
    {
        private readonly OpenGammaFudgeContext _fudgeContext;

        public FudgeMessageDecoder(OpenGammaFudgeContext fudgeContext)
        {
            _fudgeContext = fudgeContext;
        }

        public IMessage FudgeDecodeMessage(ISession session, IMessageConsumer consumer, IMessage message)
        {
            try
            {
                return new ActiveMQObjectMessage { Body = DecodeObject(message) };
            }
            catch (Exception e)
            {
                return new ActiveMQObjectMessage() {Body = e};
            }
        }

        private object DecodeObject(IMessage message)
        {
            //TODO check SEQ numbers without making this 20% slower
            byte[] content = ((IBytesMessage)message).Content;
            using (var memoryStream = new MemoryStream(content))
            {
                return _fudgeContext.DeFudgeSerialize(memoryStream);
            }
        }
    }
}