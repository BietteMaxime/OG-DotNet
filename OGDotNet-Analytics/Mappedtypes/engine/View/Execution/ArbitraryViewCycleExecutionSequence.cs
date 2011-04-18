// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArbitraryViewCycleExecutionSequence.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Model;

namespace OGDotNet.Mappedtypes.engine.View.Execution
{
    public class ArbitraryViewCycleExecutionSequence : IViewCycleExecutionSequence
    {
        private readonly Queue<ViewCycleExecutionOptions> _executionSequence;

        public static ArbitraryViewCycleExecutionSequence Of(params DateTimeOffset[] valuationTimes)
        {
            return Of((IEnumerable<DateTimeOffset>) valuationTimes);
        }

        public static ArbitraryViewCycleExecutionSequence Of(IEnumerable<DateTimeOffset> valuationTimes)
        {
            var executionSequence = valuationTimes.Select(t => new ViewCycleExecutionOptions(t, t));
            return new ArbitraryViewCycleExecutionSequence(executionSequence.ToList());
        }

        private ArbitraryViewCycleExecutionSequence(IEnumerable<ViewCycleExecutionOptions> executionSequence)
        {
            _executionSequence = new Queue<ViewCycleExecutionOptions>(executionSequence);
        }

        public bool IsEmpty
        {
            get { return ! _executionSequence.Any(); }
        }

        public ViewCycleExecutionOptions Next
        {
            get
            {
                return _executionSequence.Dequeue();
            }
        }

        public static ArbitraryViewCycleExecutionSequence FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            throw new NotImplementedException();
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            var fudgeMsg = new FudgeMsg(s.Context);

            foreach (var viewCycleExecutionOptionse in _executionSequence)
            {
                FudgeMsg msg = ((OpenGammaFudgeContext)s.Context).GetSerializer().SerializeToMsg(viewCycleExecutionOptionse);
                fudgeMsg.Add(null, null, msg);
            }

            a.Add("sequence", fudgeMsg);
        }
    }
}