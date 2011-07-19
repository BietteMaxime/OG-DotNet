//-----------------------------------------------------------------------
// <copyright file="RemoteManagableViewDefinitionRepositoryTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OGDotNet.Mappedtypes;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Mappedtypes.financial.view;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.LiveData;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using OGDotNet.Tests.Xunit.Extensions;
using Xunit;
using Xunit.Extensions;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteManagableViewDefinitionRepositoryTests : TestWithContextBase
    {
        [Xunit.Extensions.Fact]
        public void CanGet()
        {
            using (var remoteClient = Context.CreateUserClient())
            {
                Assert.NotNull(remoteClient.ViewDefinitionRepository);
            }
        }

        [Xunit.Extensions.Theory]
        [InlineData("Simples")]
        [InlineData("Spaces Spaces")]
        [InlineData("Dots.Dots")]
        [InlineData("ForwardSlash/")]
        [InlineData("BackwardSlash\\")]
        //PLAT-1108 [InlineData("Colons:")]
        //PLAT-1108 [InlineData("Equals=")]
        //PLAT-1108 [InlineData("Evil\\/.?#^5%")]
        public void CanAddAndRemovePrimitiveView(string hardUriPart)
        {
            ValueRequirement req = GetRequirement();

            using (var remoteClient = Context.CreateUserClient())
            {
                ViewDefinition vd = GetViewDefinition(req, TestUtils.GetUniqueName() + hardUriPart);

                Assert.Null(vd.UniqueID);

                remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(vd));

                var roundTripped = Context.ViewProcessor.ViewDefinitionRepository.GetViewDefinition(vd.Name);
                Assert.NotNull(roundTripped);

                AssertEquivalent(vd, roundTripped);

                remoteClient.ViewDefinitionRepository.RemoveViewDefinition(vd.Name);

                Assert.DoesNotContain(vd.Name, Context.ViewProcessor.ViewDefinitionRepository.GetDefinitionNames());
            }
        }

        [Xunit.Extensions.Fact]
        public void CanUpdatePrimitiveView()
        {
            ValueRequirement req = GetRequirement();

            using (var remoteClient = Context.CreateUserClient())
            {
                ViewDefinition vd = GetViewDefinition(req, TestUtils.GetUniqueName() + "UPDATE");

                Assert.Null(vd.UniqueID);

                remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(vd));

                var roundTripped = Context.ViewProcessor.ViewDefinitionRepository.GetViewDefinition(vd.Name);
                Assert.NotNull(roundTripped);

                AssertEquivalent(vd, roundTripped);

                remoteClient.ViewDefinitionRepository.UpdateViewDefinition(new UpdateViewDefinitionRequest(vd.Name, vd));

                remoteClient.ViewDefinitionRepository.RemoveViewDefinition(vd.Name);

                Assert.DoesNotContain(vd.Name, Context.ViewProcessor.ViewDefinitionRepository.GetDefinitionNames());
            }
        }

        [Xunit.Extensions.Fact]
        public void CantRemoveMissingView()
        {
            using (var remoteClient = Context.CreateUserClient())
            {
                Assert.Throws<DataNotFoundException>(() => remoteClient.ViewDefinitionRepository.RemoveViewDefinition(TestUtils.GetUniqueName()));
            }
        }

        [Xunit.Extensions.Fact]
        public void ViewHasRightValue()
        {
            ValueRequirement req = GetRequirement();

            using (var remoteClient = Context.CreateUserClient())
            {
                ViewDefinition vd = GetViewDefinition(req, TestUtils.GetUniqueName());

                remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(vd));

                using (var remoteViewClient = Context.ViewProcessor.CreateClient())
                {
                    var viewComputationResultModel = remoteViewClient.GetResults(vd.Name, ExecutionOptions.SingleCycle).First();
                    Assert.NotNull(viewComputationResultModel);
                    var count = viewComputationResultModel.AllResults.Where( spec => req.IsSatisfiedBy(spec.ComputedValue.Specification)).Count();
                    Assert.Equal(1, count);
                }

                remoteClient.ViewDefinitionRepository.RemoveViewDefinition(vd.Name);
            }
        }

        public class RemoteManagableViewDefinitionRepositoryRoundTripTests : ViewTestsBase
        {
            [Xunit.Extensions.Theory]
            [TypedPropertyData("FastTickingViewDefinitions")]
            public void RoundTrippedViewsInit(ViewDefinition viewDefinition)
            {
                using (var remoteClient = Context.CreateUserClient())
                {
                    viewDefinition.Name = string.Format("{0}-RoundTripped-{1}", viewDefinition.Name, Guid.NewGuid());

                    remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(viewDefinition));
                    try
                    {
                        AssertEquivalent(Context.ViewProcessor.ViewDefinitionRepository.GetViewDefinition(viewDefinition.Name), viewDefinition);
                        using (var remoteViewClient = Context.ViewProcessor.CreateClient())
                        {
                            Assert.NotNull(remoteViewClient.GetResults(viewDefinition.Name, ExecutionOptions.SingleCycle).First());
                        }
                    }
                    finally
                    {
                        remoteClient.ViewDefinitionRepository.RemoveViewDefinition(viewDefinition.Name);
                    }
                }
            }
        }

        private static void AssertEquivalent(ViewDefinition a, ViewDefinition b)
        {
            Assert.Equal(a.PortfolioIdentifier, b.PortfolioIdentifier);
            Assert.Equal(a.MaxDeltaCalcPeriod, b.MaxDeltaCalcPeriod);
            Assert.Equal(a.MinDeltaCalcPeriod, b.MinDeltaCalcPeriod);
            Assert.Equal(a.MaxFullCalcPeriod, b.MaxFullCalcPeriod);
            Assert.Equal(a.MinFullCalcPeriod, b.MinFullCalcPeriod);
            Assert.Equal(a.Name, b.Name);
            Assert.Equal(a.DefaultCurrency, b.DefaultCurrency);
            AssertEquivalent(a.User, b.User);
            AssertEquivalent(a.ResultModelDefinition, b.ResultModelDefinition);
            AssertEquivalent(a.CalculationConfigurationsByName, b.CalculationConfigurationsByName);
        }

        private static void AssertEquivalent(UserPrincipal aVal, UserPrincipal b)
        {
            Assert.Equal(aVal.IpAddress, b.IpAddress);
            Assert.Equal(aVal.UserName, b.UserName);
        }

        private static void AssertEquivalent(Dictionary<string, ViewCalculationConfiguration> a, Dictionary<string, ViewCalculationConfiguration> b)
        {
            Assert.Equal(a.Count, b.Count);
            foreach (var aEntry in a)
            {
                var aVal = aEntry.Value;
                var bVal = b[aEntry.Key];

                AssertEquivalent(aVal, bVal);
            }
        }

        private static void AssertEquivalent(ViewCalculationConfiguration aVal, ViewCalculationConfiguration bVal)
        {
            Assert.Equal(aVal.Name, bVal.Name);
            Assert.Equal(aVal.SpecificRequirements.ToList().Count, bVal.SpecificRequirements.ToList().Count);

            var matchedRequirements = aVal.SpecificRequirements.Join(bVal.SpecificRequirements, a => a, b => b, (a, b) => a, new ValueReqEquivalentComparer());
            Assert.Equal(aVal.SpecificRequirements.Count(), matchedRequirements.Count());

            Assert.Equal(aVal.PortfolioRequirementsBySecurityType.ToList().Count, bVal.PortfolioRequirementsBySecurityType.ToList().Count);
            AssertEquivalent(aVal.DeltaDefinition, bVal.DeltaDefinition);
        }

        private static void AssertEquivalent(DeltaDefinition aVal, DeltaDefinition bVal)
        {
            if (aVal == null)
            {
                Assert.Equal(null, bVal);
                return;
            }
            Assert.NotNull(bVal);

            AssertEquivalent(aVal.NumberComparer, bVal.NumberComparer);
        }

        private static void AssertEquivalent(IDeltaComparer<double> aVal, IDeltaComparer<double> bVal)
        {
            Assert.Equal(aVal, bVal);
        }

        private class ValueReqEquivalentComparer : IEqualityComparer<ValueRequirement>
        {
            public bool Equals(ValueRequirement a, ValueRequirement b)
            {
                return a.Constraints.Equals(b.Constraints) &&
                       a.TargetSpecification.Equals(b.TargetSpecification)
                       &&
                       a.ValueName.Equals(b.ValueName);
            }

            public int GetHashCode(ValueRequirement obj)
            {
                return obj.TargetSpecification.GetHashCode();
            }
        }

        private static void AssertEquivalent(ResultModelDefinition a, ResultModelDefinition b)
        {
            Assert.Equal(a.AggregatePositionOutputMode, b.AggregatePositionOutputMode);
            Assert.Equal(a.PositionOutputMode, b.PositionOutputMode);
            Assert.Equal(a.PrimitiveOutputMode, b.PrimitiveOutputMode);
            Assert.Equal(a.SecurityOutputMode, b.SecurityOutputMode);
            Assert.Equal(a.TradeOutputMode, b.TradeOutputMode);
        }

        private static ViewDefinition GetViewDefinition(ValueRequirement req, string name)
        {
            var viewDefinition = new ViewDefinition(name);

            var viewCalculationConfiguration = new ViewCalculationConfiguration("Default", new List<ValueRequirement> { req }, new Dictionary<string, HashSet<Tuple<string, ValueProperties>>>());
            viewDefinition.CalculationConfigurationsByName.Add("Default", viewCalculationConfiguration);

            return viewDefinition;
        }

        private static ValueRequirement GetRequirement()
        {
            return new ValueRequirement("Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, UniqueIdentifier.Of("BLOOMBERG_TICKER", "BP/ LN Equity")));
        }
    }
}
