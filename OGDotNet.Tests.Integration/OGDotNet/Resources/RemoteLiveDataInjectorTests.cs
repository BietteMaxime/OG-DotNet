//-----------------------------------------------------------------------
// <copyright file="RemoteLiveDataInjectorTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Linq;
using System.Threading;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Model.Resources;
using Xunit;
using FactAttribute = OGDotNet.Tests.Integration.Xunit.Extensions.FactAttribute;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteLiveDataInjectorTests : TestWithContextBase
    {
        readonly UniqueIdentifier _bloombergId = UniqueIdentifier.Of("BLOOMBERG_TICKER", "USDRG Curncy");

        [Fact]
        public void CanAddValue()
        {
            RemoteView remoteView = GetView();
            var liveDataOverrideInjector = remoteView.LiveDataOverrideInjector;

            liveDataOverrideInjector.AddValue(new ValueRequirement("Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, _bloombergId)), 100.0);
        }

        [Fact]
        public void CanRemoveValue()
        {
            RemoteView remoteView = GetView();
            var liveDataOverrideInjector = remoteView.LiveDataOverrideInjector;
            liveDataOverrideInjector.RemoveValue(new ValueRequirement("Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, _bloombergId)));
        }




        [Fact]
        public void ValueChangesResults()
        {
            RemoteView remoteView = GetView();
            var valueRequirement = new ValueRequirement("Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, _bloombergId));

            var liveDataOverrideInjector = remoteView.LiveDataOverrideInjector;
            const double newValue = 1234.5678;
            liveDataOverrideInjector.AddValue(valueRequirement, newValue);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                remoteView.Init(cancellationTokenSource.Token);
                using (var remoteViewClient = remoteView.CreateClient())
                {
                    remoteViewClient.Start();
                    while (!remoteViewClient.ResultAvailable) { }
                    var viewComputationResultModel = remoteViewClient.GetLatestResult();
                    var result = viewComputationResultModel.AllResults.Where(r => valueRequirement.IsSatisfiedBy(r.ComputedValue.Specification)).First();
                    Assert.Equal(newValue, (double)result.ComputedValue.Value);
                }
            }
        }

        [Fact]
        public void RemoveChangesResults()
        {
            RemoteView remoteView = GetView();
            var valueRequirement = new ValueRequirement("Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, _bloombergId));

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
            return CreateView(new ValueRequirement(
                "Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, _bloombergId))
                );
        }
    }
}
