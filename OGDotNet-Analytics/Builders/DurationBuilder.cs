//-----------------------------------------------------------------------
// <copyright file="DurationBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;

namespace OGDotNet.Builders
{
    static class DurationBuilder
    {
        public static TimeSpan Build(IFudgeFieldContainer msg)
        {
            return TimeSpan.FromSeconds(msg.GetLong("seconds").Value) + TimeSpan.FromTicks(msg.GetLong("nanos").Value / 100);
        }
    }
}
