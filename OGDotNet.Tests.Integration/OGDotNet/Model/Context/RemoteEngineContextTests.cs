//-----------------------------------------------------------------------
// <copyright file="RemoteEngineContextTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using OGDotNet.Model.Context;
using OGDotNet.Tests.Integration.Properties;
using Xunit;
using FactAttribute = OGDotNet.Tests.Integration.Xunit.Extensions.FactAttribute;

namespace OGDotNet.Tests.Integration.OGDotNet.Model.Context
{
    public class RemoteEngineContextTests
    {
        [Fact]
        public void CanCreate()
        {
            GetContext();
        }

        [Fact]
        public void RootUriAsExpected()
        {
            Assert.Equal(new Uri(Settings.Default.ServiceUri), GetContext().RootUri);
        }

        internal static RemoteEngineContext GetContext()
        {
            var remoteEngineContextFactory = RemoteEngineContextFactoryTests.GetContextFactory();
            return remoteEngineContextFactory.CreateRemoteEngineContext();
        }
    }
}
