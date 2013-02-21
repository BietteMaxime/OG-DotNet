// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenericBuilderBase.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Reflection;
using Fudge;
using Fudge.Serialization;

namespace OpenGamma.Fudge
{
    public abstract class GenericBuilderBase : IFudgeSerializationSurrogate
    {
        public void Serialize(object obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            if (!obj.GetType().IsGenericType)
            {
                throw new NotSupportedException("Only generic types are supported by this serialization surrogate");
            }
            Type[] genericArgs = obj.GetType().GetGenericArguments();
            MethodInfo serializeImpl = GetType().GetMethod("SerializeImpl");
            MethodInfo genericSerializeImpl = serializeImpl.MakeGenericMethod(genericArgs);
            genericSerializeImpl.Invoke(this, new[] { obj, msg, serializer });
        }

        public object Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var ret = DeserializeImpl(msg, deserializer);
            deserializer.Register(msg, ret);
            return ret;
        }

        public abstract object DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer);
    }
}
