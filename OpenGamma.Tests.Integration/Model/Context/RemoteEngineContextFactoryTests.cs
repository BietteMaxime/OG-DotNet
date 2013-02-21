// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteEngineContextFactoryTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Net;

using Castle.Core.Logging;

using OpenGamma.Fudge;
using OpenGamma.Properties;

using Xunit;

namespace OpenGamma.Model.Context
{
    public class RemoteEngineContextFactoryTests
    {
        static readonly Uri BadUri = new Uri("http://" + Guid.NewGuid());

        [Xunit.Extensions.Fact]
        public void CanCreateRemoteEngineContext()
        {
            var remoteEngineContextFactory = GetContextFactory();
            remoteEngineContextFactory.CreateRemoteEngineContext();
        }

        [Xunit.Extensions.Fact(Timeout = 30000)]
        public void CreatingContextFromSlowUriThrows()
        {
            var remoteEngineContextFactory = GetContextFactory(new Uri("http://1.1.1.1"));
            Assert.Throws<WebException>(() => remoteEngineContextFactory.CreateRemoteEngineContext());
        }

        [Xunit.Extensions.Fact]
        public void CreatingContextFromBadUriThrows()
        {
            var remoteEngineContextFactory = GetContextFactory(BadUri);
            Assert.Throws<WebException>(() => remoteEngineContextFactory.CreateRemoteEngineContext());
        }

        internal static RemoteEngineContextFactory GetContextFactory()
        {
            return GetContextFactory(new Uri(Settings.Default.ServiceUri));
        }

        private static RemoteEngineContextFactory GetContextFactory(Uri serviceUri)
        {
            var fudgeContext = new OpenGammaFudgeContext();
            return new RemoteEngineContextFactory(fudgeContext, serviceUri)
                       {
                           GlobalLogger = new ConsoleLogger(LoggerLevel.Debug)
                       };
        }
    }
}
