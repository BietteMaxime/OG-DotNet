//-----------------------------------------------------------------------
// <copyright file="RemoteLiveDataInjectorTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Linq;
using System.Threading;
using OGDotNet.Mappedtypes.Engine;
using OGDotNet.Mappedtypes.Engine.value;
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Mappedtypes.Engine.View.Execution;
using OGDotNet.Mappedtypes.Engine.View.listener;
using OGDotNet.Mappedtypes.Id;
using Xunit;
using FactAttribute = OGDotNet.Tests.Integration.Xunit.Extensions.FactAttribute;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteLiveDataInjectorTests : TestWithContextBase
    {
        static readonly UniqueIdentifier BloombergUid = UniqueIdentifier.Of("BLOOMBERG_TICKER", "USDRG Curncy");
        private static readonly Identifier BloombergId = BloombergUid.ToIdentifier();

        [Fact]
        public void CanAddValueByReq()
        {
            var defn = GetViewDefinition();
            using (var remoteClient = Context.ViewProcessor.CreateClient())
            {
                remoteClient.AttachToViewProcess(defn.Name, ExecutionOptions.RealTime);
                remoteClient.LiveDataOverrideInjector.AddValue(new ValueRequirement("Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, BloombergUid)), 100.0);
            }
        }

        [Fact]
        public void CanAddValueById()
        {
            var defn = GetViewDefinition();
            using (var remoteClient = Context.ViewProcessor.CreateClient())
            {
                remoteClient.AttachToViewProcess(defn.Name, ExecutionOptions.RealTime);
                remoteClient.LiveDataOverrideInjector.AddValue(BloombergId, "Market_Value", 100.0);
            }
        }

        [Fact]
        public void CanRemoveValueByReq()
        {
            var defn = GetViewDefinition();
            using (var remoteClient = Context.ViewProcessor.CreateClient())
            {
                remoteClient.AttachToViewProcess(defn.Name, ExecutionOptions.RealTime);
                remoteClient.LiveDataOverrideInjector.RemoveValue(new ValueRequirement("Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, BloombergUid)));
            }
        }

        [Fact]
        public void CanRemoveValueById()
        {
            var defn = GetViewDefinition();
            using (var remoteClient = Context.ViewProcessor.CreateClient())
            {
                remoteClient.AttachToViewProcess(defn.Name, ExecutionOptions.RealTime);
                remoteClient.LiveDataOverrideInjector.RemoveValue(BloombergId, "Market_Value");
            }
        }

        [Fact]
        public void ValueChangesResults()
        {
            var valueRequirement = new ValueRequirement("Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, BloombergUid));

            var defn = GetViewDefinition();
            using (var remoteClient = Context.ViewProcessor.CreateClient())
            {
                var liveDataOverrideInjector = remoteClient.LiveDataOverrideInjector;
                const double newValue = 1234.5678;

                ManualResetEvent mre = new ManualResetEvent(false);
                IViewComputationResultModel results = null;
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
            var valueRequirement = new ValueRequirement("Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, BloombergUid));

            var defn = GetViewDefinition();
            using (var remoteClient = Context.ViewProcessor.CreateClient())
            {
                var liveDataOverrideInjector = remoteClient.LiveDataOverrideInjector;
                const double newValue = 1234.5678;

                ManualResetEvent mre = new ManualResetEvent(false);
                IViewComputationResultModel results = null;
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
                "Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, BloombergUid))
                );
        }
    }
}
