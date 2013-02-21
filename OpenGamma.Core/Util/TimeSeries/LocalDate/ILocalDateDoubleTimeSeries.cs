// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILocalDateDoubleTimeSeries.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace OpenGamma.Util.TimeSeries.LocalDate
{
    public interface ILocalDateDoubleTimeSeries
    {
        LocalDateEpochDaysConverter DateTimeConverter { get; }
        IList<Tuple<DateTimeOffset, double>> Values { get; }
    }
}