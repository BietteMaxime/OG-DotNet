// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteHistoricalTimeSeriesSourceTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;

using OpenGamma.Id;
using OpenGamma.Master;
using OpenGamma.Master.Security;
using OpenGamma.Util;
using OpenGamma.Util.TimeSeries.LocalDate;

using Xunit;

namespace OpenGamma.Model.Resources
{
    public class RemoteHistoricalTimeSeriesSourceTests : RemoteEngineContextTestBase
    {
        const string DataField = "PX_LAST";
        const string DataSource = "BLOOMBERG";
        const string DataProvider = null;

        [Xunit.Extensions.Fact]
        public void CanGet()
        {
            var timeSeriesSource = Context.HistoricalTimeSeriesSource;
            Assert.NotNull(timeSeriesSource);
        }

        [Xunit.Extensions.Fact]
        public void CanGetATimeSeries()
        {
            var timeSeriesSource = Context.HistoricalTimeSeriesSource;

            var end = DateTimeOffset.Now - TimeSpan.FromDays(1);
            var start = end - TimeSpan.FromDays(14);

            ILocalDateDoubleTimeSeries series = timeSeriesSource.GetHistoricalTimeSeries(UniqueId.Create("DbHts", "3580"), start, false, end, false);
            AssertSane(series, start, end);
        }

        [Xunit.Extensions.Fact]
        public void CanGetATimeSeriesExclusive()
        {
            var timeSeriesSource = Context.HistoricalTimeSeriesSource;

            var end = DateTimeOffset.Now - TimeSpan.FromDays(1);
            var start = end - TimeSpan.FromDays(14);

            ILocalDateDoubleTimeSeries series = timeSeriesSource.GetHistoricalTimeSeries(UniqueId.Create("DbHts", "3580"), start, false, end, false);
            AssertSane(series, start, end);
            var lastIncluded = series.Values.Last().Item1;
            ILocalDateDoubleTimeSeries seriesExclusive = timeSeriesSource.GetHistoricalTimeSeries(UniqueId.Create("DbHts", "3580"), start, false, lastIncluded, false);
            Assert.Equal(series.Values.Count - 1, seriesExclusive.Values.Count);
            ILocalDateDoubleTimeSeries seriesInclusive = timeSeriesSource.GetHistoricalTimeSeries(UniqueId.Create("DbHts", "3580"), start, false, lastIncluded, true);
            Assert.Equal(series.Values.Count, seriesInclusive.Values.Count);
        }

        [Xunit.Extensions.Fact]
        public void CanGetACompleteTimeSeries()
        {
            var timeSeriesSource = Context.HistoricalTimeSeriesSource;

            var end = DateTimeOffset.Now;

            ILocalDateDoubleTimeSeries series = timeSeriesSource.GetHistoricalTimeSeries(UniqueId.Create("DbHts", "3580"));
            AssertSane(series, end);
        }

        [Xunit.Extensions.Fact]
        public void CanGetATimeSeriesByEmptyIdentifierBundle()
        {
            var timeSeriesSource = Context.HistoricalTimeSeriesSource;
            var series = timeSeriesSource.GetHistoricalTimeSeries(new ExternalIdBundle(), DateTimeOffset.Now, DataSource, DataProvider, DataField);
            Assert.NotNull(series);
        }

        [Xunit.Extensions.Fact]
        public void CanGetATimeSeriesByIdentifierBundle()
        {
            var timeSeriesSource = Context.HistoricalTimeSeriesSource;

            var result = timeSeriesSource.GetHistoricalTimeSeries(new ExternalIdBundle(new ExternalId("BLOOMBERG_BUID", "IX289029-0")), DateTimeOffset.Now, DataSource, DataProvider, DataField);
            AssertSane(result);
        }

        [Xunit.Extensions.Fact]
        public void CanGetSeriesForSomeEquities()
        {
            var timeSeriesSource = Context.HistoricalTimeSeriesSource;

            SearchResult<SecurityDocument> searchResult = GetSomeEquities();
            foreach (var securityDocument in searchResult.Documents)
            {
                var identifierBundle = securityDocument.Security.Identifiers;
                var result = timeSeriesSource.GetHistoricalTimeSeries(identifierBundle, DateTimeOffset.Now, DataSource, DataProvider, DataField);
                AssertSane(result);
            }
        }

        [Xunit.Extensions.Fact]
        public void CanGetSeriesForSomeEquitiesAlt()
        {
            var timeSeriesSource = Context.HistoricalTimeSeriesSource;

            var searchResult = GetSomeEquities();
            foreach (var securityDocument in searchResult.Documents)
            {
                var identifierBundle = securityDocument.Security.Identifiers;

                var end = DateTimeOffset.Now.Date;
                var start = end - TimeSpan.FromDays(3650);

                var result = timeSeriesSource.GetHistoricalTimeSeries(identifierBundle, DateTimeOffset.Now, DataSource, DataProvider, DataField, start, false, end, true);
                AssertSane(result);
                AssertSane(result.Item2, start, end);
            }
        }

        private static SearchResult<SecurityDocument> GetSomeEquities()
        {
            var remoteSecurityMaster = Context.SecurityMaster;
            var request = new SecuritySearchRequest(PagingRequest.First(3), "*", "EQUITY");
            return remoteSecurityMaster.Search(request);
        }

        private static void AssertSane(Tuple<UniqueId, ILocalDateDoubleTimeSeries> result)
        {
            var uniqueIdentifier = result.Item1;
            ILocalDateDoubleTimeSeries series = result.Item2;
            Assert.NotNull(uniqueIdentifier);
            AssertSane(series);
        }

        private static void AssertSane(ILocalDateDoubleTimeSeries series)
        {
            AssertSane(series, DateTimeOffset.Now);
        }

        private static void AssertSane(ILocalDateDoubleTimeSeries series, DateTimeOffset end)
        {
            AssertSane(series, DateTimeOffset.FromFileTime(0), end);
        }

        private static void AssertSane(ILocalDateDoubleTimeSeries series, DateTimeOffset start, DateTimeOffset end)
        {
            Assert.NotNull(series);
            Assert.NotNull(series.DateTimeConverter);
            Assert.NotEmpty(series.Values);

            foreach (var value in series.Values)
            {
                Assert.InRange(value.Item1, start, end);
            }

            Assert.Equal(series.Values, series.Values.OrderBy(s => s.Item1).ToList());
        }
    }
}