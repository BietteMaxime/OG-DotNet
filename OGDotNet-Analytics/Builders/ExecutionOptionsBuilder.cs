//-----------------------------------------------------------------------
// <copyright file="ExecutionOptionsBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes;
using OGDotNet.Mappedtypes.engine.View.Execution;

namespace OGDotNet.Builders
{
    class ExecutionOptionsBuilder : BuilderBase<ExecutionOptions>
    {
        public ExecutionOptionsBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override ExecutionOptions DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            throw new NotImplementedException();
        }

        protected override void SerializeImpl(ExecutionOptions obj, IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            s.WriteInline(a, "executionSequence", obj.ExecutionSequence);
            foreach (var entry in FlagFieldNames)
            {
                var flag = entry.Key;
                var fieldName = entry.Value;

                bool hasFlag = (obj.Flags & flag) != 0;

                a.Add(fieldName, hasFlag.ToString());
            }
            if (obj.MaxSuccessiveDeltaCycles != null)
            {
                a.Add("maxSuccessiveDeltaCycles", obj.MaxSuccessiveDeltaCycles);
            }
            if (obj.MarketDataSnapshotIdentifier != null)
            {
                a.Add("snapshotId", obj.MarketDataSnapshotIdentifier.ToString());
            }
        }

        static readonly Dictionary<ViewExecutionFlags, string> FlagFieldNames = BuildFlagNames();

        private static Dictionary<ViewExecutionFlags, string> BuildFlagNames()
        {
            var names = new Dictionary<ViewExecutionFlags, string>
                       {
                           {ViewExecutionFlags.TriggerCycleOnLiveDataChanged, "liveDataTriggerEnabled"},
                           {ViewExecutionFlags.TriggerCycleOnTimeElapsed, "timeElapsedTriggerEnabled"},
                           {ViewExecutionFlags.RunAsFastAsPossible, "runAsFastAsPossible"},
                           {ViewExecutionFlags.CompileOnly, "compileOnly"},
                       };
            //Sanity check the flags
            var individualFlags = new HashSet<ViewExecutionFlags>(EnumUtils.EnumValues<ViewExecutionFlags>().Where(IsIndividualFlag));
            if (!individualFlags.SetEquals(names.Keys))
            {
                individualFlags.SymmetricExceptWith(names.Keys);
                throw new OpenGammaException(string.Format("Unrepresented flags: {0}", string.Join(", ", individualFlags)));
            }
            return names;
        }

        private static bool IsIndividualFlag(ViewExecutionFlags flags)
        {
            return CountBits(flags) == 1;
        }

        private static int CountBits(ViewExecutionFlags flags)
        {
            int count = 0;
            while (flags != 0)
            {
                count++;
                flags &= flags - 1;
            }
            return count;
        }
    }
}
