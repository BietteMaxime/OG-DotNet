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

                        var value = WaitFor(enumerator, valueRequirement, o => 100.0 != (double) o );
                        Assert.NotEqual(value, 100.0);

                        liveDataOverrideInjector.AddValue(valueRequirement, 100.0);
                        value = WaitFor(enumerator, valueRequirement, o => 100.0 == (double)o);
                        Assert.Equal(value, 100.0);

                        liveDataOverrideInjector.RemoveValue(valueRequirement);
                        value = WaitFor(enumerator, valueRequirement, o => 100.0 != (double)o);
                        Assert.NotEqual(value, 100.0);
                    }
                }
            }
        }

        private static object WaitFor(IEnumerator<ViewComputationResultModel> enumerator, ValueRequirement valueRequirement, Predicate<object> match)
        {

            for (int i = 0;;i++ )
            {
                Assert.True(enumerator.MoveNext());

                object result;
                if (enumerator.Current.TryGetValue("Default", valueRequirement, out result) && match(result))
                {
                    return result;
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
