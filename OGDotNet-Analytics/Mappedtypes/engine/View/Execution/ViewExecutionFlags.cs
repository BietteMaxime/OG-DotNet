//-----------------------------------------------------------------------
// <copyright file="ViewExecutionFlags.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace OGDotNet.Mappedtypes.engine.View.Execution
{
    [Flags]
    public enum ViewExecutionFlags
    {
        /// <Summary>
        /// Indicates that a computation cycle should be triggered whenever live data inputs change. For example, this could
        /// be caused by a market data tick or an alteration to a snapshot.
        /// </Summary>
        TriggerCycleOnMarketDataChanged = 1,

        /// <Summary>
        /// Indicates that a computation cycle should be triggered after a certain time period has elapsed since the last
        /// cycle, as configured in the {@link ViewDefinition}.
        /// </Summary>
        TriggerCycleOnTimeElapsed = 2,

        /// <Summary>
        /// Indicates that the execution sequence should proceed as fast as possible, ignoring any minimum elapsed time
        /// between cycles specified in the view definition, and possibly executing cycles concurrently.
        /// </Summary>
        RunAsFastAsPossible = 4,

        /// <Summary>
        /// Indicates that the view definition should be compiled but not executed.
        /// </Summary>
        CompileOnly = 8,

        /// <summary>
        ///  Indicates that all market data should be present before a cycle is allowed to run.
        /// </summary>
        AwaitMarketData = 16,
        TriggersEnabled = TriggerCycleOnMarketDataChanged | TriggerCycleOnTimeElapsed
    }
}