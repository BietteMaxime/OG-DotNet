// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ForwardCurve.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge;
using Fudge.Serialization;

namespace OpenGamma.Analytics.Financial.Model.InterestRate.Curve
{
    public class ForwardCurve
    {
        private readonly Math.Curve.Curve _forwardCurve;

        public ForwardCurve(Math.Curve.Curve forwardCurve)
        {
            _forwardCurve = forwardCurve;
        }

        public Math.Curve.Curve FwdCurve
        {
            // NOTE: 'nice' name used in java not allowed here
            get { return _forwardCurve; }
        }

        public static ForwardCurve FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new ForwardCurve(deserializer.FromField<Math.Curve.Curve>(ffc.GetByName("forwardCurve")));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}
