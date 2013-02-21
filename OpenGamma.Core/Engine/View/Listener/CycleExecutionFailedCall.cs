// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CycleExecutionFailedCall.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Engine.View.Execution;

namespace OpenGamma.Engine.View.Listener
{
    public class CycleExecutionFailedCall
    {
        private readonly ViewCycleExecutionOptions _executionOptions;
        private readonly JavaException _exception;

        public CycleExecutionFailedCall(ViewCycleExecutionOptions executionOptions, JavaException exception)
        {
            _executionOptions = executionOptions;
            _exception = exception;
        }

        public ViewCycleExecutionOptions ExecutionOptions
        {
            get { return _executionOptions; }
        }

        public JavaException Exception
        {
            get { return _exception; }
        }

        public static CycleExecutionFailedCall FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new CycleExecutionFailedCall(deserializer.FromField<ViewCycleExecutionOptions>(ffc.GetByName("executionOptions")), deserializer.FromField<JavaException>(ffc.GetByName("exception")));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}
