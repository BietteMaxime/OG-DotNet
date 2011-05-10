// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExecutionOptions.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Id;

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
            return Batch(ArbitraryViewCycleExecutionSequence.Of(valuationTime));
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

        public static IViewExecutionOptions Snapshot(UniqueIdentifier snapshotIdentifier)
        {
            return new ExecutionOptions(ArbitraryViewCycleExecutionSequence.Of(DateTimeOffset.Now), true, false, null, false, snapshotIdentifier);
        }

        private readonly IViewCycleExecutionSequence _executionSequence;
        private readonly bool _runAsFastAsPossible;
        private readonly bool _liveDataTriggerEnabled;
        private readonly int? _maxSuccessiveDeltaCycles;
        private readonly bool _compileOnly;
        private readonly UniqueIdentifier _marketDataSnapshotIdentifier;

        public ExecutionOptions(IViewCycleExecutionSequence executionSequence, bool runAsFastAsPossible, bool liveDataTriggerEnabled, int? maxSuccessiveDeltaCycles, bool compileOnly, UniqueIdentifier marketDataSnapshotIdentifier = null)
        {
            _executionSequence = executionSequence;
            _runAsFastAsPossible = runAsFastAsPossible;
            _liveDataTriggerEnabled = liveDataTriggerEnabled;
            _maxSuccessiveDeltaCycles = maxSuccessiveDeltaCycles;
            _compileOnly = compileOnly;
            _marketDataSnapshotIdentifier = marketDataSnapshotIdentifier;
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

        public UniqueIdentifier MarketDataSnapshotIdentifier
        {
            get { return _marketDataSnapshotIdentifier; }
        }

        public static ExecutionOptions FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            throw new NotImplementedException();
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            s.WriteInline(a, "executionSequence", ExecutionSequence);
            a.Add("runAsFastAsPossible", RunAsFastAsPossible);
            a.Add("liveDataTriggerEnabled", LiveDataTriggerEnabled);
            if (MaxSuccessiveDeltaCycles != null)
            {
                a.Add("maxSuccessiveDeltaCycles", MaxSuccessiveDeltaCycles);
            }
            a.Add("compileOnly", CompileOnly);
            if (MarketDataSnapshotIdentifier != null)
            {
                a.Add("snapshotId", MarketDataSnapshotIdentifier.ToString());
            }
        }
    }
}