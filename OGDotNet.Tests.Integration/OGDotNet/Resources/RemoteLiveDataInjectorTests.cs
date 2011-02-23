using System;
using System.Collections.Generic;
using System.Threading;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Model.Resources;
using Xunit;
using FactAttribute = OGDotNet.Tests.Integration.Xunit.Extensions.FactAttribute;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteLiveDataInjectorTests : TestWithContextBase
    {
        [Fact]
        public void CanInjectValue()
        {
            RemoteView remoteView = GetView();
            var liveDataOverrideInjector = remoteView.LiveDataOverrideInjector;
            liveDataOverrideInjector.AddValue(GetRequirement(), 100.0);
        }

        [Fact]
        public void CanDeleteValue()
        {
            RemoteView remoteView = GetView();
            var liveDataOverrideInjector = remoteView.LiveDataOverrideInjector;
            liveDataOverrideInjector.RemoveValue(GetRequirement());
        }




        [Fact]
        public void ValueChangesResultsOnSwapTestView()
        {
            RemoteView remoteView = GetView();
            var valueRequirement = GetRequirement();

            var liveDataOverrideInjector = remoteView.LiveDataOverrideInjector;
            liveDataOverrideInjector.RemoveValue(valueRequirement);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                remoteView.Init(cancellationTokenSource.Token);
                using (var remoteViewClient = remoteView.CreateClient())
                {
                    var viewComputationResultModels = remoteViewClient.GetResults(cancellationTokenSource.Token);
                    using (var enumerator = viewComputationResultModels.GetEnumerator())
                    {

                        WaitFor(enumerator, valueRequirement, o => 100.0 != (double)o);

                        liveDataOverrideInjector.AddValue(valueRequirement, 100.0);
                        WaitFor(enumerator, valueRequirement, o => 100.0 == (double) o);

                        liveDataOverrideInjector.RemoveValue(valueRequirement);
                        WaitFor(enumerator, valueRequirement, o => 100.0 != (double) o);
                    }
                }
            }
        }


        private RemoteView GetView()
        {
            return Context.ViewProcessor.GetView("Swap Test View");
        }
        private static ValueRequirement GetRequirement()
        {
            return new ValueRequirement("Market_Value", new ComputationTargetSpecification(ComputationTargetType.Primitive, UniqueIdentifier.Parse("BLOOMBERG_TICKER::USDRG Curncy")));
        }

        private static void  WaitFor(IEnumerator<ViewComputationResultModel> enumerator, ValueRequirement valueRequirement, Predicate<object> match)
        {

            for (int i = 0;;i++ )
            {
                Assert.True(enumerator.MoveNext());

                var viewComputationResultModel = enumerator.Current;

                if (GetContains(viewComputationResultModel, valueRequirement, match))
                    return;
                Assert.InRange(i,0,100);
            }
        }

        private static bool GetContains(ViewComputationResultModel viewComputationResultModel, ValueRequirement valueRequirement, Predicate<object> match)
        {
            bool contains = false;
            object result;
            if (viewComputationResultModel.TryGetValue("Default", valueRequirement, out result) && match(result))
            {
                contains = true;
            }
            return contains;
        }
    }
}
