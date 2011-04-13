//-----------------------------------------------------------------------
// <copyright file="RemoteEngineContextFactoryTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using OGDotNet.Model;
using OGDotNet.Model.Context;
using OGDotNet.Tests.Integration.Properties;
using FactAttribute = OGDotNet.Tests.Integration.Xunit.Extensions.FactAttribute;

namespace OGDotNet.Tests.Integration.OGDotNet.Model.Context
{
    public class RemoteEngineContextFactoryTests
    {
        [Fact]
        public void CanCreateRemoteEngineContext()
        {
            GetContextFactory();
        }

        internal static RemoteEngineContextFactory GetContextFactory()
        {
            var fudgeContext = new OpenGammaFudgeContext();
            Uri serviceUri = new Uri(Settings.Default.ServiceUri);
            string configId = Settings.Default.ConfigId;

            return new RemoteEngineContextFactory(fudgeContext, serviceUri, configId);
        }
    }
}
