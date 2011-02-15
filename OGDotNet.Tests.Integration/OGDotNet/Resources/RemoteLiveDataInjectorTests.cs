using System;
using System.Collections.Generic;
using System.Threading;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.Id;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteLiveDataInjectorTests : TestWithContextBase
    {
        [Fact]
        public void CanInjectValue()
        {
            var remoteView = Context.ViewProcessor.GetView("Swap Test View");
            var liveDataOverrideInjector = remoteView.LiveDataOverrideInjector;
            liveDataOverrideInjector.AddValue(GetRequirement(), 100.0);
        }

        [Fact]
        public void CanDeleteValue()
        {
            var remoteView = Context.ViewProcessor.GetView("Swap Test View");
            var liveDataOverrideInjector = remoteView.LiveDataOverrideInjector;
            liveDataOverrideInjector.RemoveValue(GetRequirement());
        }


        [Fact]
        public void ValueChangesResults()
        {
            var valueRequirement = GetRequirement();

             var remoteView = Context.ViewProcessor.GetView("Swap Test View");
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

                        WaitFor(enumerator, valueRequirement, o => 100.0 != (double) o );

                        liveDataOverrideInjector.AddValue(valueRequirement, 100.0);
                        WaitFor(enumerator, valueRequirement, o => 100.0 == (double)o);
                        WaitFor(enumerator, valueRequirement, o => 100.0 == (double)o);

                        liveDataOverrideInjector.RemoveValue(valueRequirement);
                        WaitFor(enumerator, valueRequirement, o => 100.0 != (double)o);
                        WaitFor(enumerator, valueRequirement, o => 100.0 != (double)o);
                    }
                }
            }
        }

        private static void  WaitFor(IEnumerator<ViewComputationResultModel> enumerator, ValueRequirement valueRequirement, Predicate<object> match)
        {

            for (int i = 0;;i++ )
            {
                Assert.True(enumerator.MoveNext());

                object result;
                if (enumerator.Current.TryGetValue("Default", valueRequirement, out result) && match(result))
                {
                    return;
                }
                Assert.InRange<int>(i,0,100);
            }
        }

        private static ValueRequirement GetRequirement()
        {
            return new ValueRequirement("Market_Value",new ComputationTargetSpecification(ComputationTargetType.Primitive,UniqueIdentifier.Parse("BLOOMBERG_TICKER::USBG5 Curncy")));
        }
    }
}
