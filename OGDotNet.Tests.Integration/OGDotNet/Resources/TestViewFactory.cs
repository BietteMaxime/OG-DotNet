﻿//-----------------------------------------------------------------------
// <copyright file="TestViewFactory.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using OGDotNet.Mappedtypes.Engine.Value;
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Mappedtypes.Financial.View;
using OGDotNet.Model.Context;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using OGDotNet.Utils;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class TestViewFactory : DisposableBase
    {
        private readonly ConcurrentQueue<Tuple<RemoteEngineContext, string>> _createdViews = new ConcurrentQueue<Tuple<RemoteEngineContext, string>>();
        public ViewDefinition CreateViewDefinition(RemoteEngineContext context, ValueRequirement valueRequirement)
        {
            var viewDefinition = new ViewDefinition(TestUtils.GetUniqueName());
            viewDefinition.CalculationConfigurationsByName.Add("Default", new ViewCalculationConfiguration("Default", new List<ValueRequirement> { valueRequirement }, new Dictionary<string, HashSet<Tuple<string, ValueProperties>>>()));
            using (var remoteClient = context.CreateFinancialClient())
            {
                var uid = remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(viewDefinition));
                viewDefinition.UniqueID = uid;
            }
            _createdViews.Enqueue(Tuple.Create(context, viewDefinition.Name));
            return viewDefinition;
        }

        protected override void Dispose(bool disposing)
        {
            Tuple<RemoteEngineContext, string> tup;
            while (_createdViews.TryDequeue(out tup))
            {
                using (var remoteClient = tup.Item1.CreateFinancialClient())
                {
                    remoteClient.ViewDefinitionRepository.RemoveViewDefinition(tup.Item2);
                }
            }
        }
    }
}