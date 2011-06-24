// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExecutionOptions.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.engine.View.Execution
{
    [FudgeSurrogate(typeof(ExecutionOptionsBuilder))]
    public class ExecutionOptions : IViewExecutionOptions
    {
        public static IViewExecutionOptions RealTime
        {
            get
            {
                return new ExecutionOptions(new RealTimeViewCycleExecutionSequence(), ViewExecutionFlags.TriggersEnabled);
            }
        }

        public static IViewExecutionOptions SingleCycle
        {
            get { return GetSingleCycle(DateTimeOffset.Now); }
        }

        public static IViewExecutionOptions GetSingleCycle(DateTimeOffset valuationTime)
        {
            return Batch(ArbitraryViewCycleExecutionSequence.Of(valuationTime));
        }

        public static IViewExecutionOptions Batch(IViewCycleExecutionSequence cycleExecutionSequence)
        {
            return new ExecutionOptions(cycleExecutionSequence, ViewExecutionFlags.RunAsFastAsPossible);
        }

        public static IViewExecutionOptions GetCompileOnly()
        {
            return GetCompileOnly(ArbitraryViewCycleExecutionSequence.Of(DateTimeOffset.Now));
        }

        public static IViewExecutionOptions GetCompileOnly(IViewCycleExecutionSequence cycleExecutionSequence)
        {
            return new ExecutionOptions(cycleExecutionSequence, ViewExecutionFlags.CompileOnly);
        }

        public static IViewExecutionOptions Snapshot(UniqueIdentifier snapshotIdentifier)
        {
            return new ExecutionOptions(new RealTimeViewCycleExecutionSequence(), ViewExecutionFlags.TriggersEnabled, marketDataSnapshotIdentifier:snapshotIdentifier);
        }

        private readonly IViewCycleExecutionSequence _executionSequence;
        private readonly ViewExecutionFlags _flags;
        private readonly int? _maxSuccessiveDeltaCycles;
        private readonly UniqueIdentifier _marketDataSnapshotIdentifier;

        public ExecutionOptions(IViewCycleExecutionSequence executionSequence, ViewExecutionFlags flags, int? maxSuccessiveDeltaCycles = null, UniqueIdentifier marketDataSnapshotIdentifier = null)
        {
            _executionSequence = executionSequence;
            _flags = flags;
            _maxSuccessiveDeltaCycles = maxSuccessiveDeltaCycles;
            _marketDataSnapshotIdentifier = marketDataSnapshotIdentifier;
        }

        public IViewCycleExecutionSequence ExecutionSequence
        {
            get { return _executionSequence; }
        }

        public ViewExecutionFlags Flags
        {
            get { return _flags; }
        }

        public int? MaxSuccessiveDeltaCycles
        {
            get { return _maxSuccessiveDeltaCycles; }
        }

        public UniqueIdentifier MarketDataSnapshotIdentifier
        {
            get { return _marketDataSnapshotIdentifier; }
        }
    }
}