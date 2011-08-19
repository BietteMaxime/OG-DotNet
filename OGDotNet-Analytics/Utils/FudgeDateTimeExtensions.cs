//-----------------------------------------------------------------------
// <copyright file="FudgeDateTimeExtensions.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge.Types;

namespace OGDotNet.Utils
{
    static class FudgeDateTimeExtensions
    {
        public static DateTimeOffset ToDateTimeOffsetWithDefault(this FudgeDateTime dt)
        {
            return dt == null ? default(DateTimeOffset) : dt.ToDateTimeOffset();
        }

        public static FudgeDateTime ToFudgeDateTimeOffsetWithDefault(this DateTimeOffset dt)
        {
            return dt == default(DateTimeOffset) ? null : new FudgeDateTime(dt);
        }
    }
}
