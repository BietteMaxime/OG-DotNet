//-----------------------------------------------------------------------
// <copyright file="BuilderBase.xaml.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Builders
{
    internal abstract class BuilderBase<T> : IFudgeSerializationSurrogate
    {
        protected readonly FudgeContext Context;

        protected BuilderBase(FudgeContext context, Type type)
        {
            if (type != typeof(T))
            {
                throw new ArgumentException("Type paramter doesn't match generic parameter", "type");
            }
            Context = context;
        }

        public void Serialize(object obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            SerializeImpl((T)obj, msg, serializer);
        }

        protected virtual void SerializeImpl(T obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
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