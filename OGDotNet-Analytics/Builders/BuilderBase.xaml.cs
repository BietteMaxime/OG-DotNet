using System;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Builders
{
    internal abstract class BuilderBase<T>: IFudgeSerializationSurrogate
    {
        protected readonly FudgeContext Context;
        protected readonly Type Type;

        protected BuilderBase(FudgeContext context, Type type)
        {
            Context = context;
            Type = type;
        }

        public void Serialize(object obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            SerializeImpl(obj, msg, serializer);
        }

        protected virtual void SerializeImpl(object obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public object Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var ret = DeserializeImpl(msg, deserializer);
            deserializer.Register(msg, ret);
            return ret;
        }

        public abstract T DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer);
    }
}