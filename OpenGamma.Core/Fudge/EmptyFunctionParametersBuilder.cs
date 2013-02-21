// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EmptyFunctionParametersBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Engine.Function;

namespace OpenGamma.Fudge
{
    /// <remarks>
    /// Exists to stop reflection based builder being used
    /// </remarks>
    class EmptyFunctionParametersBuilder : BuilderBase<EmptyFunctionParameters>
    {
        public EmptyFunctionParametersBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override EmptyFunctionParameters DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return new EmptyFunctionParameters();
        }

        protected override void SerializeImpl(EmptyFunctionParameters obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
        }
    }
}
