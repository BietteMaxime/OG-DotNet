//-----------------------------------------------------------------------
// <copyright file="RemoteHistoricalTimeSeriesSourceTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Util.Db;
using OGDotNet.Mappedtypes.Util.Timeseries.Localdate;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteHistoricalTimeSeriesSourceTests : TestWithContextBase
    {
        [Xunit.Extensions.Fact]
        public void CanGet()
        {
            var timeSeriesSource = Context.HistoricalTimeSeriesSource;
            Assert.NotNull(timeSeriesSource);
        }

        [FactAttribute]
        public void CanGetATimeSeries()
        {
            var timeSeriesSource = Context.HistoricalTimeSeriesSource;

            var end = DateTimeOffset.Now - TimeSpan.FromDays(1);
            var start = end - TimeSpan.FromDays(7);

            ILocalDateDoubleTimeSeries series = timeSeriesSource.GetHistoricalTimeSeries(UniqueIdentifier.Of("Tss", "3580"), start, false, end, true);
            AssertSane(series, start, end);
        }

        [FactAttribute]
        public void CanGetACompleteTimeSeries()
        {
            var timeSeriesSource = Context.HistoricalTimeSeriesSource;

            var end = DateTimeOffset.Now;

            ILocalDateDoubleTimeSeries series = timeSeriesSource.GetHistoricalTimeSeries(UniqueIdentifier.Of("Tss", "3580"));
            AssertSane(series, end);
        }

        [FactAttribute]
        public void CantGetATimeSeriesByEmptyIdentifierBundle()
        {
            var timeSeriesSource = Context.HistoricalTimeSeriesSource;
            Assert.Throws<ArgumentException>(() => timeSeriesSource.GetHistoricalTimeSeries(new IdentifierBundle()));
        }

        [FactAttribute]
        public void CanGetATimeSeriesByIdentifierBundle()
        {
            var timeSeriesSource = Context.HistoricalTimeSeriesSource;

            var result = timeSeriesSource.GetHistoricalTimeSeries(new IdentifierBundle(new Identifier("BLOOMBERG_BUID", "IX289029-0")));
            AssertSane(result);
        }

        [FactAttribute]
        public void CanGetSeriesForSomeFutures()
        {
            var timeSeriesSource = Context.HistoricalTimeSeriesSource;

            var remoteSecurityMaster = Context.SecurityMaster;
            var searchResult = remoteSecurityMaster.Search("*", "FUTURE", new PagingRequest(1, 10));
            foreach (var securityDocument in searchResult.Documents)
            {
                var identifierBundle = securityDocument.Security.Identifiers;
                var result = timeSeriesSource.GetHistoricalTimeSeries(identifierBundle);
                AssertSane(result);
            }
        }
        [FactAttribute]
        public void CanGetSeriesForSomeFuturesAlt()
        {
            var timeSeriesSource = Context.HistoricalTimeSeriesSource;

            var remoteSecurityMaster = Context.SecurityMaster;
            var searchResult = remoteSecurityMaster.Search("*", "FUTURE", new PagingRequest(1, 10));
            foreach (var securityDocument in searchResult.Documents)
            {
                var identifierBundle = securityDocument.Security.Identifiers;

                var end = DateTimeOffset.Now.Date;
                var start = end - TimeSpan.FromDays(3650);

                var result = timeSeriesSource.GetHistoricalTimeSeries(identifierBundle, start, false, end, true);
                AssertSane(result);
                AssertSane(result.Item2, start, end);
            }
        }

        private static void AssertSane(Tuple<UniqueIdentifier, ILocalDateDoubleTimeSeries> result)
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