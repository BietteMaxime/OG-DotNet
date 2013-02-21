// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DurationBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge;

namespace OpenGamma.Fudge
{
    static class DurationBuilder
    {
        public static TimeSpan? Build(IFudgeFieldContainer msg)
        {
            if (msg == null)
            {
                return null;
            }

            return TimeSpan.FromSeconds(msg.GetLong("seconds").Value) + TimeSpan.FromTicks(msg.GetLong("nanos").Value / 100);
        }
    }
}
