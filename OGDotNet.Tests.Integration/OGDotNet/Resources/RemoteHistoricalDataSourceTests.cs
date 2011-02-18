using System;
using OGDotNet.Mappedtypes.Id;
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

            ILocalDateDoubleTimeSeries series = historicalDataSource.GetHistoricalData(UniqueIdentifier.Parse("Tss::3580"), start, false, end,true);
            AssertSane(series, start, end);
        }

        [FactAttribute]
        public void CanGetACompleteTimeSeries()
        {
            var historicalDataSource = Context.HistoricalDataSource;

            var end = DateTimeOffset.Now;

            ILocalDateDoubleTimeSeries series = historicalDataSource.GetHistoricalData(UniqueIdentifier.Parse("Tss::3580"));
            AssertSane(series, end);
        }

        [FactAttribute]
        public void CantGetATimeSeriesByEmptyIdentifierBundle()
        {
            var historicalDataSource = Context.HistoricalDataSource;
            var response = historicalDataSource.GetHistoricalData(new IdentifierBundle());
            Assert.Null(response.Item1);
            Assert.Null(response.Item2);
        }

        [FactAttribute]
        public void CanGetATimeSeriesByIdentifierBundle()
        {
            var historicalDataSource = Context.HistoricalDataSource;

            var end = DateTimeOffset.Now;
            var result = historicalDataSource.GetHistoricalData(new IdentifierBundle(new Identifier("BLOOMBERG_BUID", "IX289029-0")));
            var uniqueIdentifier = result.Item1;
            ILocalDateDoubleTimeSeries series = result.Item2;
            Assert.NotNull(uniqueIdentifier);
            AssertSane(series, end);
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
                Assert.InRange(value.Item1, DateTime.FromFileTimeUtc(0), end);
            }
        }
    }
}