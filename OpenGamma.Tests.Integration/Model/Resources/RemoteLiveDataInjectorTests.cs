// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteLiveDataInjectorTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;
using System.Threading;
using OpenGamma.Core.Config.Impl;
using OpenGamma.Engine;
using OpenGamma.Engine.Target;
using OpenGamma.Engine.Value;
using OpenGamma.Engine.View;
using OpenGamma.Engine.View.Execution;
using OpenGamma.Engine.View.Listener;
using OpenGamma.Financial.User;
using OpenGamma.Id;
using OpenGamma.Master.Config;
using OpenGamma.Xunit.Extensions;
using Xunit;

namespace OpenGamma.Model.Resources
{
    public class RemoteLiveDataInjectorTests : RemoteEngineContextTestBase
    {
        static readonly UniqueId BloombergUid = UniqueId.Create("BLOOMBERG_TICKER", "USDRG Curncy");
        private static readonly ExternalId BloombergId = BloombergUid.ToIdentifier();

        [Xunit.Extensions.Fact]
        public void CanAddValueByReq()
        {
            using (var financialClient = Context.CreateFinancialClient())
            using (var viewClient = Context.ViewProcessor.CreateViewClient())
            {
                var viewDefId = CreateTestViewDefinition(financialClient);
                viewClient.AttachToViewProcess(viewDefId, ExecutionOptions.RealTime);
                viewClient.LiveDataOverrideInjector.AddValue(new ValueRequirement("Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, BloombergUid)), 100.0);
            }
        }

        [Xunit.Extensions.Fact]
        public void CanAddValueById()
        {
            using (var financialClient = Context.CreateFinancialClient())
            using (var viewClient = Context.ViewProcessor.CreateViewClient())
            {
                var viewDefId = CreateTestViewDefinition(financialClient);
                viewClient.AttachToViewProcess(viewDefId, ExecutionOptions.RealTime);
                viewClient.LiveDataOverrideInjector.AddValue(BloombergId, "Market_Value", 100.0);
            }
        }

        [Xunit.Extensions.Fact]
        public void CanRemoveValueByReq()
        {
            using (var financialClient = Context.CreateFinancialClient())
            using (var viewClient = Context.ViewProcessor.CreateViewClient())
            {
                var viewDefId = CreateTestViewDefinition(financialClient);
                viewClient.AttachToViewProcess(viewDefId, ExecutionOptions.RealTime);
                viewClient.LiveDataOverrideInjector.RemoveValue(new ValueRequirement("Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, BloombergUid)));
            }
        }

        [Xunit.Extensions.Fact]
        public void CanRemoveValueById()
        {
            using (var financialClient = Context.CreateFinancialClient())
            using (var viewClient = Context.ViewProcessor.CreateViewClient())
            {
                var viewDefId = CreateTestViewDefinition(financialClient);
                viewClient.AttachToViewProcess(viewDefId, ExecutionOptions.RealTime);
                viewClient.LiveDataOverrideInjector.RemoveValue(BloombergId, "Market_Value");
            }
        }

        [Xunit.Extensions.Fact]
        public void ValueChangesResults()
        {
            var valueRequirement = new ValueRequirement("Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, BloombergUid));

            using (var financialClient = Context.CreateFinancialClient())
            using (var viewClient = Context.ViewProcessor.CreateViewClient())
            {
                var viewDefId = CreateTestViewDefinition(financialClient);
                var liveDataOverrideInjector = viewClient.LiveDataOverrideInjector;
                const double newValue = 1234.5678;

                var mre = new ManualResetEvent(false);
                IViewComputationResultModel results = null;
                var listener = new EventViewResultListener();
                listener.CycleCompleted += delegate(object sender, CycleCompletedArgs e)
                                               {
                                                   results = e.FullResult;
                                                   mre.Set();
                                               };
                viewClient.SetResultListener(listener);
                viewClient.AttachToViewProcess(viewDefId, ExecutionOptions.RealTime);
                liveDataOverrideInjector.AddValue(valueRequirement, newValue);

                mre.WaitOne();
                var result = results.AllResults.First(r => valueRequirement.IsSatisfiedBy(r.ComputedValue.Specification));
                Assert.Equal(newValue, (double) result.ComputedValue.Value);
            }
        }

        [Xunit.Extensions.Fact]
        public void RemoveChangesResults()
        {
            var valueRequirement = new ValueRequirement("Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, BloombergUid));

            using (var financialClient = Context.CreateFinancialClient())
            using (var viewClient = Context.ViewProcessor.CreateViewClient())
            {
                var viewDefId = CreateTestViewDefinition(financialClient);
                var liveDataOverrideInjector = viewClient.LiveDataOverrideInjector;
                const double newValue = 1234.5678;

                var mre = new ManualResetEvent(false);
                IViewComputationResultModel results = null;
                var listener = new EventViewResultListener();
                listener.CycleCompleted += delegate(object sender, CycleCompletedArgs e)
                {
                    results = e.FullResult;
                    mre.Set();
                };
                viewClient.SetResultListener(listener);
                viewClient.AttachToViewProcess(viewDefId, ExecutionOptions.RealTime);
                liveDataOverrideInjector.AddValue(valueRequirement, newValue);
                liveDataOverrideInjector.RemoveValue(valueRequirement);
                
                mre.WaitOne();
                mre.Reset();

                var result = results.AllResults.First(r => valueRequirement.IsSatisfiedBy(r.ComputedValue.Specification));
                Assert.NotEqual(newValue, (double)result.ComputedValue.Value);
            }
        }

        private UniqueId CreateTestViewDefinition(FinancialClient financialClient)
        {
            var calcConfig = new ViewCalculationConfiguration("Default");
            var valueRequirement = new ValueRequirement("Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, BloombergUid));
            calcConfig.AddSpecificRequirement(valueRequirement);
            
            var viewDefinition = new ViewDefinition(TestUtils.GetUniqueName());
            viewDefinition.CalculationConfigurationsByName.Add("Default", calcConfig);
            
            var configItem = new ConfigItem<ViewDefinition>(viewDefinition, viewDefinition.Name);
            var configDoc = new ConfigDocument<ViewDefinition>(configItem);
            return financialClient.ConfigMaster.Add(configDoc).UniqueId;
        }
    }
}
