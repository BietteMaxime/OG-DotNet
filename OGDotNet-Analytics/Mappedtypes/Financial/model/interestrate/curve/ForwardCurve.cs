//-----------------------------------------------------------------------
// <copyright file="ForwardCurve.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using CCurve = OGDotNet.Mappedtypes.Math.Curve.Curve;

namespace OGDotNet.Mappedtypes.Financial.Model.Interestrate.Curve
{
    public class ForwardCurve
    {
        private readonly CCurve _forwardCurve;

        public ForwardCurve(CCurve forwardCurve)
        {
            _forwardCurve = forwardCurve;
        }

        public CCurve FwdCurve //NOTE: 'nice' name used in java not allowed here
        {
            get { return _forwardCurve; }
        }

        public static ForwardCurve FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new ForwardCurve(deserializer.FromField<CCurve>(ffc.GetByName("forwardCurve")));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}
