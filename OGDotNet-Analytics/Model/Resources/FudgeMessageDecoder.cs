//-----------------------------------------------------------------------
// <copyright file="FudgeMessageDecoder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.IO;
using System.Threading;
using Apache.NMS;
using Apache.NMS.ActiveMQ.Commands;

namespace OGDotNet.Model.Resources
{
    public class FudgeMessageDecoder
    {
        private readonly OpenGammaFudgeContext _fudgeContext;

        private long _lastSequenceNumber = -1;

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
                return new ActiveMQObjectMessage {Body = e};
            }
        }

        private object DecodeObject(IMessage message)
        {
            byte[] content = ((IBytesMessage)message).Content;
            using (var memoryStream = new MemoryStream(content))
            {
                var fudgeMsgEnvelope = _fudgeContext.Deserialize(memoryStream);

                long? seqNumber = fudgeMsgEnvelope.Message.GetLong("#");
                if (!seqNumber.HasValue)
                {
                    throw new ArgumentException("Couldn't find sequence number");
                }
                long expectedSeqNumber = Interlocked.Increment(ref _lastSequenceNumber);
                if (expectedSeqNumber != seqNumber.Value)
                {
                    throw new ArgumentException(string.Format("Unexpected SEQ number {0} expected {1}", seqNumber, expectedSeqNumber));
                }

                return _fudgeContext.DeFudgeSerialize(fudgeMsgEnvelope.Message);
            }
        }
    }
}