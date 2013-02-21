// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewTestBase.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Xunit;

namespace OpenGamma.Model.Resources
{
    public abstract class ViewTestBase : RemoteEngineContextTestBase, IUseFixture<ViewTestFixture>
    {
        public ViewTestFixture Fixture { get; private set; }

        public void SetFixture(ViewTestFixture data)
        {
            Fixture = data;
        }
    }
}