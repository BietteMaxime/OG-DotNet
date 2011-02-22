using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.financial.view;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.LiveData;
using Xunit;

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

        [Xunit.Extensions.Fact]
        public void CanAddPrimitiveView()
        {
            ValueRequirement req = GetRequirement();

            using (var remoteClient = Context.CreateUserClient())
            {
                ViewDefinition vd = GetViewDefinition(req);
                
                remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(vd));

                var roundTripped = Context.ViewProcessor.GetView(vd.Name);
                Assert.NotNull(roundTripped);

                AssertEquivalent(vd, roundTripped.Definition);
            }
        }

        private static void AssertEquivalent(ViewDefinition a, ViewDefinition b)
        {
            Assert.Equal(a.PortfolioIdentifier, b.PortfolioIdentifier);
            Assert.Equal(a.MaxDeltaCalcPeriod, b.MaxDeltaCalcPeriod);
            Assert.Equal(a.MinDeltaCalcPeriod, b.MinDeltaCalcPeriod);
            Assert.Equal(a.MaxFullCalcPeriod, b.MaxFullCalcPeriod);
            Assert.Equal(a.MinFullCalcPeriod, b.MinFullCalcPeriod);
            Assert.Equal(a.Name,b.Name);
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

        private static void AssertEquivalent(Dictionary<string, ViewCalculationConfiguration> a, Dictionary<string, ViewCalculationConfiguration>  b)
        {
            Assert.Equal(a.Count, b.Count);
            foreach (var aEntry in a)
            {
                var aVal = aEntry.Value;
                var bVal = b[aEntry.Key];

                AssertEquivalent(aVal,bVal);
            }
        }

        private static void AssertEquivalent(ViewCalculationConfiguration aVal, ViewCalculationConfiguration bVal)
        {
            Assert.Equal(aVal.Name,bVal.Name);
            Assert.Equal(aVal.SpecificRequirements.ToList().Count, bVal.SpecificRequirements.ToList().Count);
            foreach (var tuple in aVal.SpecificRequirements.Zip(bVal.SpecificRequirements, Tuple.Create))
            {
                AssertEquivalent(tuple.Item1,tuple.Item2);
            }

            //TODO stricter asserts

            Assert.Equal(aVal.PortfolioRequirementsBySecurityType.ToList().Count, bVal.PortfolioRequirementsBySecurityType.ToList().Count);

        }

        private static void AssertEquivalent(ValueRequirement a, ValueRequirement b)
        {
            //TODO stricter asserts
            Assert.Equal(a.Constraints.Properties.Count, b.Constraints.Properties.Count);
            Assert.Equal(a.TargetSpecification, b.TargetSpecification);
            Assert.Equal(a.ValueName, b.ValueName);
        }

        private static void AssertEquivalent(ResultModelDefinition a, ResultModelDefinition b)
        {
            Assert.Equal(a.AggregatePositionOutputMode,b.AggregatePositionOutputMode);
            Assert.Equal(a.PositionOutputMode, b.PositionOutputMode);
            Assert.Equal(a.PrimitiveOutputMode, b.PrimitiveOutputMode);
            Assert.Equal(a.SecurityOutputMode, b.SecurityOutputMode);
            Assert.Equal(a.TradeOutputMode, b.TradeOutputMode);
        }

        private static ViewDefinition GetViewDefinition(ValueRequirement req)
        {
            var viewDefinition = new ViewDefinition( Guid.NewGuid().ToString());

            var viewCalculationConfiguration = new ViewCalculationConfiguration("Default", new List<ValueRequirement> { req }, new Dictionary<string, ValueProperties>());
            viewDefinition.CalculationConfigurationsByName.Add("Default", viewCalculationConfiguration);

            return viewDefinition;
        }

        private static ValueRequirement GetRequirement()
        {
            return new ValueRequirement("Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, UniqueIdentifier.Parse("BLOOMBERG_TICKER::BP/ LN Equity")));
        }
    }
}
