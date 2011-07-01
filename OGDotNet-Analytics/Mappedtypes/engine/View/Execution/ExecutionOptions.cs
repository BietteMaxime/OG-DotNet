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
using OGDotNet.Mappedtypes.engine.marketdata.spec;
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
                return new ExecutionOptions(new InfiniteViewCycleExecutionSequence(), ViewExecutionFlags.TriggersEnabled, null, new ViewCycleExecutionOptions(default(DateTimeOffset), GetDefaultMarketDataSpec()));
            }
        }

        public static IViewExecutionOptions SingleCycle
        {
            get { return GetSingleCycle(GetDefaultMarketDataSpec()); }
        }

        public static LiveMarketDataSpecification GetDefaultMarketDataSpec()
        {
            return new LiveMarketDataSpecification();
        }

        public static IViewExecutionOptions GetSingleCycle(MarketDataSpecification marketDataSpecification)
        {
            return GetSingleCycle(DateTimeOffset.Now, marketDataSpecification);
        }

        public static IViewExecutionOptions GetSingleCycle(DateTimeOffset valuationTime, MarketDataSpecification marketDataSpecification)
        {
            return Batch(ArbitraryViewCycleExecutionSequence.Of(valuationTime), new ViewCycleExecutionOptions(valuationTime, marketDataSpecification));
        }

        public static IViewExecutionOptions Batch(IViewCycleExecutionSequence cycleExecutionSequence)
        {
            return Batch(cycleExecutionSequence, null);
        }

        public static IViewExecutionOptions Batch(IViewCycleExecutionSequence cycleExecutionSequence, ViewCycleExecutionOptions defaultCycleOptions)
        {
            return new ExecutionOptions(cycleExecutionSequence, ViewExecutionFlags.RunAsFastAsPossible, defaultExecutionOptions:defaultCycleOptions);
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
            return new ExecutionOptions(new InfiniteViewCycleExecutionSequence(), ViewExecutionFlags.TriggersEnabled, defaultExecutionOptions: new ViewCycleExecutionOptions(default(DateTimeOffset), new UserMarketDataSpecification(snapshotIdentifier)));
        }

        private readonly IViewCycleExecutionSequence _executionSequence;
        private readonly ViewExecutionFlags _flags;
        private readonly int? _maxSuccessiveDeltaCycles;
        private readonly ViewCycleExecutionOptions _defaultExecutionOptions;

        public ExecutionOptions(IViewCycleExecutionSequence executionSequence, ViewExecutionFlags flags, int? maxSuccessiveDeltaCycles = null, ViewCycleExecutionOptions defaultExecutionOptions = null)
        {
            _executionSequence = executionSequence;
            _flags = flags;
            _maxSuccessiveDeltaCycles = maxSuccessiveDeltaCycles;
            _defaultExecutionOptions = defaultExecutionOptions;
        }

        public IViewCycleExecutionSequence ExecutionSequence
        {
            get { return _executionSequence; }
        }

        public ViewExecutionFlags Flags
        {
            get { return _flags; }
        }

        public ViewCycleExecutionOptions DefaultExecutionOptions
        {
            get { return _defaultExecutionOptions; }
        }

        public int? MaxSuccessiveDeltaCycles
        {
            get { return _maxSuccessiveDeltaCycles; }
        }
    }
}