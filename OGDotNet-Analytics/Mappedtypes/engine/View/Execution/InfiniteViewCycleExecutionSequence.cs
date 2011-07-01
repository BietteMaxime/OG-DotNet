// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InfiniteViewCycleExecutionSequence.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.engine.View.Execution
{
    public class InfiniteViewCycleExecutionSequence : IViewCycleExecutionSequence
    {
        public bool IsEmpty
        {
            get { return false; }
        }

        public ViewCycleExecutionOptions Next
        {
            get { return new ViewCycleExecutionOptions(DateTimeOffset.Now, null); }
        }

        public static InfiniteViewCycleExecutionSequence FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new InfiniteViewCycleExecutionSequence();
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            s.WriteTypeHeader(a, typeof(InfiniteViewCycleExecutionSequence));
        }
    }
}