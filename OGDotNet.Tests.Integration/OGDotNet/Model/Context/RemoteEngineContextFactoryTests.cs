//-----------------------------------------------------------------------
// <copyright file="RemoteEngineContextFactoryTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Net;
using OGDotNet.Mappedtypes;
using OGDotNet.Model;
using OGDotNet.Model.Context;
using OGDotNet.Tests.Integration.Properties;
using Xunit;
using FactAttribute = OGDotNet.Tests.Integration.Xunit.Extensions.FactAttribute;

namespace OGDotNet.Tests.Integration.OGDotNet.Model.Context
{
    public class RemoteEngineContextFactoryTests
    {
        static readonly Uri BadUri = new Uri("http://" + Guid.NewGuid());

        [Fact]
        public void CanCreateRemoteEngineContext()
        {
            var remoteEngineContextFactory = GetContextFactory();
            remoteEngineContextFactory.CreateRemoteEngineContext();
        }

        [Fact(Timeout = 30000)]
        public void CreatingContextFromSlowUriThrows()
        {
            var remoteEngineContextFactory = GetContextFactory(new Uri("http://1.1.1.1"), "SomeConfig");
            Assert.Throws<WebException>(() => remoteEngineContextFactory.CreateRemoteEngineContext());
        }

        [Fact]
        public void CreatingContextFromBadConfigThrows()
        {
            var configId = Guid.NewGuid().ToString();
            var remoteEngineContextFactory = GetContextFactory(new Uri(Settings.Default.ServiceUri), configId);
            var exception = Assert.Throws<OpenGammaException>(() => remoteEngineContextFactory.CreateRemoteEngineContext());
            Assert.Equal("Missing config " + configId, exception.Message);
        }
        [Fact]
        public void CreatingContextFromBadUriThrows()
        {
            var remoteEngineContextFactory = GetContextFactory(BadUri, "SomeConfig");
            Assert.Throws<WebException>(() => remoteEngineContextFactory.CreateRemoteEngineContext());
        }

        internal static RemoteEngineContextFactory GetContextFactory()
        {
            return GetContextFactory(new Uri(Settings.Default.ServiceUri), Settings.Default.ConfigId);
        }

        private static RemoteEngineContextFactory GetContextFactory(Uri serviceUri, string configId)
        {
            var fudgeContext = new OpenGammaFudgeContext();
            return new RemoteEngineContextFactory(fudgeContext, serviceUri, configId);
        }
    }
}
