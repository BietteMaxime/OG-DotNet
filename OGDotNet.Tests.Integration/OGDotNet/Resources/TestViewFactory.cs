using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.financial.view;
using OGDotNet.Model.Context;
using OGDotNet.Model.Resources;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using OGDotNet.Utils;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class TestViewFactory : DisposableBase
    {
        private readonly ConcurrentQueue<Tuple<RemoteEngineContext, string>> _createdViews = new ConcurrentQueue<Tuple<RemoteEngineContext, string>>();
        public RemoteView CreateView(RemoteEngineContext context, ValueRequirement valueRequirement)
        {
            var viewDefinition = new ViewDefinition(TestUtils.GetUniqueName());
            viewDefinition.CalculationConfigurationsByName.Add("Default", new ViewCalculationConfiguration("Default", new List<ValueRequirement> { valueRequirement }, new Dictionary<string, HashSet<Tuple<string, ValueProperties>>>()));
            using (var remoteClient = context.CreateUserClient())
            {
                remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(viewDefinition));
            }
            var remoteView = context.ViewProcessor.GetView(viewDefinition.Name);
            _createdViews.Enqueue(Tuple.Create(context, remoteView.Name));
            return remoteView;
        }

        protected override void Dispose(bool disposing)
        {
            Tuple<RemoteEngineContext, string> tup;
            while (_createdViews.TryDequeue(out tup))
            {
                using (var remoteClient = tup.Item1.CreateUserClient())
                {
                    remoteClient.ViewDefinitionRepository.RemoveViewDefinition(tup.Item2);
                }
            }
        }
    }
}