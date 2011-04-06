using System;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Util.Db;
using OGDotNet.Mappedtypes.Util.Timeseries.Localdate;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteHistoricalDataSourceTests : TestWithContextBase
    {
        [Xunit.Extensions.Fact]
        public void CanGet()
        {
            var historicalDataSource = Context.HistoricalDataSource;
            Assert.NotNull(historicalDataSource);
        }

        [FactAttribute]
        public void CanGetATimeSeries()
        {
            var historicalDataSource = Context.HistoricalDataSource;
            
            var end = DateTimeOffset.Now;
            var start = end - TimeSpan.FromDays(7);

            ILocalDateDoubleTimeSeries series = historicalDataSource.GetHistoricalData(UniqueIdentifier.Of("Tss","3580"), start, false, end,true);
            AssertSane(series, start, end);
        }

        [FactAttribute]
        public void CanGetACompleteTimeSeries()
        {
            var historicalDataSource = Context.HistoricalDataSource;

            var end = DateTimeOffset.Now;

            ILocalDateDoubleTimeSeries series = historicalDataSource.GetHistoricalData(UniqueIdentifier.Of("Tss","3580"));
            AssertSane(series, end);
        }

        [FactAttribute]
        public void CantGetATimeSeriesByEmptyIdentifierBundle()
        {
            var historicalDataSource = Context.HistoricalDataSource;
            Assert.Throws<ArgumentException>(() => historicalDataSource.GetHistoricalData(new IdentifierBundle()));
            
        }

        [FactAttribute]
        public void CanGetATimeSeriesByIdentifierBundle()
        {
            var historicalDataSource = Context.HistoricalDataSource;

            var end = DateTimeOffset.Now;
            var result = historicalDataSource.GetHistoricalData(new IdentifierBundle(new Identifier("BLOOMBERG_BUID", "IX289029-0")));
            AssertSane(result);
        }

        [FactAttribute]
        public void CanGetSeriesForSomeFutures()
        {
            var historicalDataSource = Context.HistoricalDataSource;


            var remoteSecurityMaster = Context.SecurityMaster;
            var searchResult = remoteSecurityMaster.Search("*","FUTURE",new PagingRequest(1,10));
            foreach (var securityDocument in searchResult.Documents)
            {
                var identifierBundle = securityDocument.Security.Identifiers;
                var result = historicalDataSource.GetHistoricalData(identifierBundle);
                AssertSane(result);
            }
        }
        [FactAttribute]
        public void CanGetSeriesForSomeFuturesAlt()
        {
            var historicalDataSource = Context.HistoricalDataSource;


            var remoteSecurityMaster = Context.SecurityMaster;
            var searchResult = remoteSecurityMaster.Search("*", "FUTURE", new PagingRequest(1, 10));
            foreach (var securityDocument in searchResult.Documents)
            {
                var identifierBundle = securityDocument.Security.Identifiers;
                
                var end = DateTimeOffset.Now.Date;
                var start = end-TimeSpan.FromDays(3650);

                var result = historicalDataSource.GetHistoricalData(identifierBundle,start, false, end, true);
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
            AssertSane(series,DateTimeOffset.FromFileTime(0),end);
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
        }
    }
}