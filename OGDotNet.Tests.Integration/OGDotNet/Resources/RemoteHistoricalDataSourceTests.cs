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
            Assert.NotNull(series);
            Assert.NotNull(series.DateTimeConverter);
            Assert.NotEmpty(series.Values);

            foreach (var value in series.Values)
            {
                Assert.InRange(value.Item1, start,end);
            }
        }
    }
}