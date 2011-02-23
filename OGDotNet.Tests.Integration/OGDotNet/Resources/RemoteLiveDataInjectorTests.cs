using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.financial.view;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Model.Resources;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using Xunit;
using FactAttribute = OGDotNet.Tests.Integration.Xunit.Extensions.FactAttribute;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteLiveDataInjectorTests : TestWithContextBase
    {
        [Fact]
        public void CanAddValue()
        {
            RemoteView remoteView = GetView();
            var liveDataOverrideInjector = remoteView.LiveDataOverrideInjector;
            liveDataOverrideInjector.AddValue(GetRequirement(), 100.0);
        }

        [Fact]
        public void CanRemoveValue()
        {
            RemoteView remoteView = GetView();
            var liveDataOverrideInjector = remoteView.LiveDataOverrideInjector;
            liveDataOverrideInjector.RemoveValue(GetRequirement());
        }




        [Fact]
        public void ValueChangesResults()
        {
            RemoteView remoteView = GetView();
            var valueRequirement = GetRequirement();

            var liveDataOverrideInjector = remoteView.LiveDataOverrideInjector;
            const double newValue = 1234.5678;
            liveDataOverrideInjector.AddValue(valueRequirement, newValue);
            
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                remoteView.Init(cancellationTokenSource.Token);
                using (var remoteViewClient = remoteView.CreateClient())
                {
                    remoteViewClient.Start();
                    while (! remoteViewClient.ResultAvailable){}
                    var viewComputationResultModel = remoteViewClient.GetLatestResult();
                    var result=viewComputationResultModel.AllResults.Where(r => valueRequirement.IsSatisfiedBy(r.ComputedValue.Specification)).First();
                    Assert.Equal(newValue, (double) result.ComputedValue.Value);
                }
            }
        }

        [Fact]
        public void RemoveChangesResults()
        {
            RemoteView remoteView = GetView();
            var valueRequirement = GetRequirement();

            var liveDataOverrideInjector = remoteView.LiveDataOverrideInjector;
            const double newValue = 1234.5678;
            
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                liveDataOverrideInjector.AddValue(valueRequirement, newValue);
                liveDataOverrideInjector.RemoveValue(valueRequirement);
                remoteView.Init(cancellationTokenSource.Token);

                using (var remoteViewClient = remoteView.CreateClient())
                {
                    remoteViewClient.Start();
                    while (!remoteViewClient.ResultAvailable) { }
                    var viewComputationResultModel = remoteViewClient.GetLatestResult();
                    var result = viewComputationResultModel.AllResults.Where(r => valueRequirement.IsSatisfiedBy(r.ComputedValue.Specification)).First();
                    Assert.NotEqual(newValue, (double)result.ComputedValue.Value);
                }
            }
        }


        private RemoteView GetView()
        {
            var viewDefinition = new ViewDefinition(TestUtils.GetUniqueName());
            viewDefinition.CalculationConfigurationsByName.Add("Default", new ViewCalculationConfiguration("Default", new List<ValueRequirement>(){GetRequirement()},new Dictionary<string, ValueProperties>() ));
            using (var remoteClient = Context.CreateUserClient())
            {
                remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(viewDefinition));
            }
            return Context.ViewProcessor.GetView(viewDefinition.Name);
        }
        private static ValueRequirement GetRequirement()
        {
            return new ValueRequirement("Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, UniqueIdentifier.Parse("BLOOMBERG_TICKER::USDRG Curncy")));
        }
    }
}
