// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VolatilityCubeTest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

using OpenGamma.Core.Config.Impl;
using OpenGamma.Engine;
using OpenGamma.Engine.MarketData.Spec;
using OpenGamma.Engine.Target;
using OpenGamma.Engine.Value;
using OpenGamma.Engine.View;
using OpenGamma.Engine.View.Execution;
using OpenGamma.Master.Config;
using OpenGamma.Model.Resources;
using OpenGamma.Util.Money;
using OpenGamma.Util.Time;
using OpenGamma.Util.Tuple;
using OpenGamma.Xunit.Extensions;

using Xunit;
using Xunit.Extensions;

namespace OpenGamma.MarketDataSnapshot
{
    public class VolatilityCubeTest : RemoteEngineContextTestBase
    {
        [Xunit.Extensions.Theory]
        [InlineData("USD")]
        public void CanGetUsdVolatilityCube(string currencyName)
        {
            const string cubeName = "BLOOMBERG";
            var valueProperties = ValueProperties.Create(new Dictionary<string, ISet<string>> { { "Cube", new HashSet<string> { cubeName } } }, new HashSet<string>());

            var calcConfig = new ViewCalculationConfiguration("Default");
            var volCubeRequirement = new ValueRequirement("VolatilityCubeMarketData", new ComputationTargetSpecification(ComputationTargetType.Primitive, Currency.Create(currencyName).UniqueId), valueProperties);
            calcConfig.AddSpecificRequirement(volCubeRequirement);
            
            var vdName = string.Join("-", TestUtils.GetUniqueName(), cubeName, currencyName);

            var defn = new ViewDefinition(vdName, new ResultModelDefinition(ResultOutputMode.TerminalOutputs))
                {
                    MaxFullCalcPeriod = TimeSpan.FromSeconds(1)
                };
            defn.AddCalculationConfiguration(calcConfig);
            using (var remoteClient = Context.CreateFinancialClient())
            {
                var item = ConfigItem.Create(defn, defn.Name);
                var doc = new ConfigDocument<ViewDefinition>(item);
                doc = remoteClient.ConfigMaster.Add(doc);
                defn = doc.Config.Value;
                using (var remoteViewClient = Context.ViewProcessor.CreateViewClient())
                {
                    var marketDataSpecifications = new List<MarketDataSpecification> {new LiveMarketDataSpecification()};
                    var viewComputationResultModels = remoteViewClient.GetResults(defn.UniqueId, new ExecutionOptions(new InfiniteViewCycleExecutionSequence(), ViewExecutionFlags.AwaitMarketData | ViewExecutionFlags.TriggersEnabled, null, new ViewCycleExecutionOptions(default(DateTimeOffset), marketDataSpecifications)));
                    int i = 0;

                    foreach (var viewComputationResultModel in viewComputationResultModels)
                    {
                        if (viewComputationResultModel.AllLiveData.Any())
                        {
                            var liveDataCount = viewComputationResultModel.AllLiveData.Count();
                            if (liveDataCount > 10 && liveDataCount == i)
                            {
                                var volatilityCubeData = (VolatilityCubeData) viewComputationResultModel.AllResults.Single().ComputedValue.Value;
                                Assert.InRange(volatilityCubeData.DataPoints.Count, 1, int.MaxValue);
                                Assert.InRange(volatilityCubeData.Strikes.Count, 1, int.MaxValue);
                                Assert.Empty(volatilityCubeData.OtherData.DataPoints);

                                var actual = volatilityCubeData.DataPoints.Count + volatilityCubeData.OtherData.DataPoints.Count + volatilityCubeData.Strikes.Count;
                                Assert.InRange(actual, liveDataCount * 0.5, liveDataCount); // Allow 50% for PLAT-1383

                                var pays = volatilityCubeData.DataPoints.Where(k => k.Key.RelativeStrike < 0);
                                var recvs = volatilityCubeData.DataPoints.Where(k => k.Key.RelativeStrike > 0);
                                Assert.NotEmpty(pays);
                                Assert.NotEmpty(volatilityCubeData.DataPoints.Where(k => k.Key.RelativeStrike == 0));
                                Assert.NotEmpty(recvs);
                                    
                                foreach (var dataPoint in volatilityCubeData.DataPoints.Keys)
                                {
                                    var strike = volatilityCubeData.Strikes[GetStrikeKey(dataPoint)];
                                    Assert.True(strike > 0.0);
                                }

                                break;
                            }

                            i = liveDataCount;
                        }
                    }
                }
            }
        }

        private static Pair<Tenor, Tenor> GetStrikeKey(VolatilityPoint dataPoint)
        {
            return new Pair<Tenor, Tenor>(dataPoint.SwapTenor, dataPoint.OptionExpiry);
        }
    }
}
