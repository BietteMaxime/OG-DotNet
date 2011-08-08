//-----------------------------------------------------------------------
// <copyright file="EmptyFunctionParametersBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Engine.Function;

namespace OGDotNet.Builders
{
    /// <remarks>
    /// Exists to stop reflection based builder being used
    /// </remarks>
    class EmptyFunctionParametersBuilder : BuilderBase<EmptyFunctionParameters>
    {
        public EmptyFunctionParametersBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override EmptyFunctionParameters DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return new EmptyFunctionParameters();
        }

        protected override void SerializeImpl(EmptyFunctionParameters obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
        }
    }
}
