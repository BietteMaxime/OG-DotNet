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
using OGDotNet.Mappedtypes.Math.Curve;

namespace OGDotNet.Mappedtypes.Financial.model.interestrate.curve
{
    class ForwardCurve
    {
        private readonly Curve _forwardCurve;

        public ForwardCurve(Curve forwardCurve)
        {
            _forwardCurve = forwardCurve;
        }

        public Curve FwdCurve //NOTE: 'nice' name used in java not allowed here
        {
            get { return _forwardCurve; }
        }

        public static ForwardCurve FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new ForwardCurve(deserializer.FromField<Curve>(ffc.GetByName("forwardCurve")));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}
