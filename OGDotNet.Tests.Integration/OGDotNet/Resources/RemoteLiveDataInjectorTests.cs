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
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Mappedtypes.engine.View.listener;
using OGDotNet.Mappedtypes.Id;
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
            var defn = GetViewDefinition();
            using (var remoteClient = Context.ViewProcessor.CreateClient())
            {
                remoteClient.AttachToViewProcess(defn.Name, ExecutionOptions.RealTime);
                remoteClient.LiveDataOverrideInjector.AddValue(new ValueRequirement("Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, _bloombergId)), 100.0);
            }
        }

        [Fact]
        public void CanRemoveValue()
        {
            var defn = GetViewDefinition();
            using (var remoteClient = Context.ViewProcessor.CreateClient())
            {
                remoteClient.AttachToViewProcess(defn.Name, ExecutionOptions.RealTime);
                remoteClient.LiveDataOverrideInjector.RemoveValue(new ValueRequirement("Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, _bloombergId)));
            }
        }

        [Fact]
        public void ValueChangesResults()
        {
            var valueRequirement = new ValueRequirement("Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, _bloombergId));

            var defn = GetViewDefinition();
            using (var remoteClient = Context.ViewProcessor.CreateClient())
            {
                var liveDataOverrideInjector = remoteClient.LiveDataOverrideInjector;
                const double newValue = 1234.5678;

                ManualResetEvent mre = new ManualResetEvent(false);
                InMemoryViewComputationResultModel results = null;
                var listener = new EventViewResultListener();
                listener.CycleCompleted += delegate(object sender, CycleCompletedArgs e)
                                               {
                                                   results = e.FullResult;
                                                   mre.Set();
                                               };
                remoteClient.SetResultListener(listener);
                remoteClient.AttachToViewProcess(defn.Name, ExecutionOptions.RealTime);
                liveDataOverrideInjector.AddValue(valueRequirement, newValue);

                mre.WaitOne();
                var result = results.AllResults.Where(
                        r => valueRequirement.IsSatisfiedBy(r.ComputedValue.Specification)).First();
                Assert.Equal(newValue, (double) result.ComputedValue.Value);
            }
        }

        [Fact]
        public void RemoveChangesResults()
        {
            var valueRequirement = new ValueRequirement("Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, _bloombergId));

            var defn = GetViewDefinition();
            using (var remoteClient = Context.ViewProcessor.CreateClient())
            {
                var liveDataOverrideInjector = remoteClient.LiveDataOverrideInjector;
                const double newValue = 1234.5678;

                ManualResetEvent mre = new ManualResetEvent(false);
                InMemoryViewComputationResultModel results = null;
                var listener = new EventViewResultListener();
                listener.CycleCompleted += delegate(object sender, CycleCompletedArgs e)
                {
                    results = e.FullResult;
                    mre.Set();
                };
                remoteClient.SetResultListener(listener);
                remoteClient.AttachToViewProcess(defn.Name, ExecutionOptions.RealTime);
                liveDataOverrideInjector.AddValue(valueRequirement, newValue);
                liveDataOverrideInjector.RemoveValue(valueRequirement);
                
                mre.WaitOne();
                mre.Reset();

                var result = results.AllResults.Where(r => valueRequirement.IsSatisfiedBy(r.ComputedValue.Specification)).First();
                Assert.NotEqual(newValue, (double)result.ComputedValue.Value);
            }
        }

        private ViewDefinition GetViewDefinition()
        {
            return CreateViewDefinition(new ValueRequirement(
                "Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, _bloombergId))
                );
        }
    }
}
