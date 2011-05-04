//-----------------------------------------------------------------------
// <copyright file="TestWithContextBase.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Model.Context;
using OGDotNet.Tests.Integration.OGDotNet.Model.Context;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class TestWithContextBase : IUseFixture<TestViewFactory>
    {
        private static readonly Lazy<RemoteEngineContext> ContextLazy = new Lazy<RemoteEngineContext>(GetContext);
        protected static RemoteEngineContext Context
        { 
            get { return ContextLazy.Value; }
        }

        private static RemoteEngineContext GetContext()
        {
            return RemoteEngineContextTests.GetContext();
        }

        private TestViewFactory _testViewFactory;

        public void SetFixture(TestViewFactory testViewFactory)
        {
            _testViewFactory = testViewFactory;
        }

        protected ViewDefinition CreateViewDefinition(ValueRequirement valueRequirement)
        {
            return _testViewFactory.CreateViewDefinition(Context, valueRequirement);
        }
    }
}