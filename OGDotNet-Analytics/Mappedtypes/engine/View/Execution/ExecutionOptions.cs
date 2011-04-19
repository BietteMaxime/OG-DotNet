// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExecutionOptions.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace OGDotNet.Mappedtypes.engine.View.Execution
{
    public class ExecutionOptions : IViewExecutionOptions
    {
        public static IViewExecutionOptions RealTime
        {
            get
            {
                return new ExecutionOptions(new RealTimeViewCycleExecutionSequence(), false, true, null, false);
            }
        }

        public static IViewExecutionOptions SingleCycle
        {
            get { return GetSingleCycle(DateTimeOffset.Now); }
        }

        public static IViewExecutionOptions GetSingleCycle(DateTimeOffset valuationTime)
        {
            return new ExecutionOptions(
                ArbitraryViewCycleExecutionSequence.Of(valuationTime),
                                    true,
                                    false,
                                    null,
                                    false);
        }

        public static IViewExecutionOptions Batch(IViewCycleExecutionSequence cycleExecutionSequence)
        {
            return new ExecutionOptions(
                cycleExecutionSequence,
                true,
                false,
                null,
                false);
        }

        private readonly IViewCycleExecutionSequence _executionSequence;
        private readonly bool _runAsFastAsPossible;
        private readonly bool _liveDataTriggerEnabled;
        private readonly int? _maxSuccessiveDeltaCycles;
        private readonly bool _compileOnly;

        public ExecutionOptions(IViewCycleExecutionSequence executionSequence, bool runAsFastAsPossible, bool liveDataTriggerEnabled, int? maxSuccessiveDeltaCycles, bool compileOnly)
        {
            _executionSequence = executionSequence;
            _runAsFastAsPossible = runAsFastAsPossible;
            _liveDataTriggerEnabled = liveDataTriggerEnabled;
            _maxSuccessiveDeltaCycles = maxSuccessiveDeltaCycles;
            _compileOnly = compileOnly;
        }

        public IViewCycleExecutionSequence ExecutionSequence
        {
            get { return _executionSequence; }
        }

        public bool RunAsFastAsPossible
        {
            get { return _runAsFastAsPossible; }
        }

        public bool LiveDataTriggerEnabled
        {
            get { return _liveDataTriggerEnabled; }
        }

        public int? MaxSuccessiveDeltaCycles
        {
            get { return _maxSuccessiveDeltaCycles; }
        }

        public bool CompileOnly
        {
            get { return _compileOnly; }
        }
    }
}