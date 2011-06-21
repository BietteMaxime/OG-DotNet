﻿//-----------------------------------------------------------------------
// <copyright file="VolatilityCubeTest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Mappedtypes.Core.Common;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.depGraph.DependencyGraph;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Mappedtypes.financial.view;
using OGDotNet.Mappedtypes.Util.Time;
using OGDotNet.Mappedtypes.Util.tuple;
using OGDotNet.Tests.Integration.OGDotNet.Resources;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using Xunit;
using Xunit.Extensions;

namespace OGDotNet.Tests.Integration.OGDotNet.Core.marketdatasnapshot
{
    public class VolatilityCubeTest : TestWithContextBase
    {
        [Xunit.Extensions.Theory]
        [InlineData("DEFAULT")]
        [InlineData("BLOOMBERG")]
        public void CanGetUSDVolatilityCube(string cubeName)
        {
            var valueProperties = ValueProperties.Create(new Dictionary<string, ISet<string>> { { "Cube", new HashSet<string> { cubeName } } }, new HashSet<string>());

            var viewCalculationConfiguration = new ViewCalculationConfiguration("Default",
                new[]
                    {
                        new ValueRequirement("VolatilityCubeMarketData", new ComputationTargetSpecification(ComputationTargetType.Primitive, Currency.USD.UniqueId), valueProperties)
                    }, new Dictionary<string, HashSet<Tuple<string, ValueProperties>>>());
            
            var vdName = TestUtils.GetUniqueName() + cubeName;

            var defn = new ViewDefinition(vdName, new ResultModelDefinition(ResultOutputMode.TerminalOutputs),
                               calculationConfigurationsByName:
                                   new Dictionary<string, ViewCalculationConfiguration> {{"Default", viewCalculationConfiguration}}, maxFullCalcPeriod:TimeSpan.FromSeconds(1));
            using (var remoteClient = Context.CreateUserClient())
            {
                remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(defn));
                try
                {
                    var remoteViewClient = Context.ViewProcessor.CreateClient();
                    var viewComputationResultModels = remoteViewClient.GetResults(defn.Name, ExecutionOptions.RealTime);
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

                                var actual = volatilityCubeData.DataPoints.Count + volatilityCubeData.Strikes.Count;
                                Assert.InRange(actual, liveDataCount * 0.9, liveDataCount); //Allow 10% for PLAT-1383

                                var pays = volatilityCubeData.DataPoints.Where(k => k.Key.RelativeStrike < 0);
                                var recvs = volatilityCubeData.DataPoints.Where(k => k.Key.RelativeStrike > 0);
                                Assert.NotEmpty(pays);
                                Assert.NotEmpty(volatilityCubeData.DataPoints.Where(k => k.Key.RelativeStrike == 0));
                                Assert.NotEmpty(recvs);

                                Assert.InRange(pays.Count(), recvs.Count() / 2.0, recvs.Count() * 2.0); //Disallow completely unbalanced cubes
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
                finally
                {
                    remoteClient.ViewDefinitionRepository.RemoveViewDefinition(defn.Name);
                }
            }
        }

        private Pair<Tenor, Tenor> GetStrikeKey(VolatilityPoint dataPoint)
        {
            return new Pair<Tenor, Tenor>(dataPoint.SwapTenor, dataPoint.OptionExpiry);
        }
    }
}