//-----------------------------------------------------------------------
// <copyright file="StreamingFudgeBuilderBase.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Encodings;
using Fudge.Serialization;
using OGDotNet.Model;

namespace OGDotNet.Builders.Streaming
{
    internal abstract class StreamingFudgeBuilderBase<T> : IStreamingFudgeBuilder where T : class
    {
        public Type Type { get { return typeof(T); } }

        public TReq Deserialize<TReq>(OpenGammaFudgeContext context, IFudgeStreamReader stream, SerializationTypeMap typeMap)
        {
            if (typeof(TReq) != typeof(T))
            {
                throw new ArgumentException(string.Format("Unexpected type {0}", typeof(TReq)));
            }
            return (TReq)(object)Deserialize(context, stream, typeMap);
        }

        protected abstract T Deserialize(OpenGammaFudgeContext context, IFudgeStreamReader stream, SerializationTypeMap typeMap);

        protected T1 DeserializeStandard<T1>(OpenGammaFudgeContext context, IFudgeStreamReader reader, SerializationTypeMap typeMap)
        {
            //Called just after SubmessageFieldStart for a field containing a T1
            //TODO: should handle T1 being stream serializable
            FudgeMsg dequeueMessage = ReadOneSubmessage(context, reader);
            var fudgeSerializer = new FudgeSerializer(context, typeMap);
            return fudgeSerializer.Deserialize<T1>(dequeueMessage);
        }

        private static FudgeMsg ReadOneSubmessage(OpenGammaFudgeContext context, IFudgeStreamReader reader)
        {
            var writer = new FudgeMsgStreamWriter(context);

            writer.StartMessage();

            int depth = 1;
            while (reader.HasNext)
            {
                switch (reader.MoveNext())
                {
                    case FudgeStreamElement.MessageStart:
                        throw new ArgumentException();
                    case FudgeStreamElement.MessageEnd:
                        throw new ArgumentException();
                    case FudgeStreamElement.SimpleField:
                        writer.WriteField(reader.FieldName, reader.FieldOrdinal, reader.FieldType, reader.FieldValue);
                        break;
                    case FudgeStreamElement.SubmessageFieldStart:
                        depth++;
                        writer.StartSubMessage(reader.FieldName, reader.FieldOrdinal);
                        break;
                    case FudgeStreamElement.SubmessageFieldEnd:
                        depth--;
                        if (depth == 0)
                        {
                            writer.EndMessage();
                            return writer.DequeueMessage();
                        }
                        writer.EndSubMessage();
                        break;
                    default:
                        break;      // Unknown
                }
            }

            throw new ArgumentException();
        }
    }
}