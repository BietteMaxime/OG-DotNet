//-----------------------------------------------------------------------
// <copyright file="TestWithContextBase.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Model.Context;
using OGDotNet.Model.Resources;
using OGDotNet.Tests.Integration.OGDotNet.Model.Context;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class TestWithContextBase : IUseFixture<TestViewFactory>
    {
        private static RemoteEngineContext _context;
        protected static RemoteEngineContext Context
        { 
            get
            {
                _context = _context ?? GetContext();
                return _context;
            }
        }

        protected static RemoteEngineContext GetContext()
        {
            return RemoteEngineContextTests.GetContext();
        }

        private TestViewFactory _testViewFactory;

        public void SetFixture(TestViewFactory testViewFactory)
        {
            _testViewFactory = testViewFactory;
        }


        protected RemoteView CreateView(ValueRequirement valueRequirement)
        {
            return _testViewFactory.CreateView(Context, valueRequirement);
        }
    }
}