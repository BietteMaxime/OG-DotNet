// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenGammaModuleTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Castle.Windsor.Installer;

using OpenGamma.Fudge;
using OpenGamma.Model;
using OpenGamma.Model.Context;

using Xunit;

namespace OpenGamma
{
    public class OpenGammaModuleTests
    {
        [Fact]
        public void CanLoadModuleExplicit()
        {
            var container = new WindsorContainer();

            var defaultConfigurationStore = new DefaultConfigurationStore();
            var openGammaModule = new OpenGammaModule();
            openGammaModule.Install(container, defaultConfigurationStore);
            
            AssertResolvable(container);
        }

        [Fact]
        public void CanLoadModuleImplicit()
        {
            var container = new WindsorContainer();

            var defaultConfigurationStore = new DefaultConfigurationStore();
            FromAssembly.Containing<RemoteEngineContextFactory>().Install(container, defaultConfigurationStore);

            AssertResolvable(container);
        }

        private static void AssertResolvable(WindsorContainer container)
        {
            var openGammaFudgeContext = container.Resolve<OpenGammaFudgeContext>();
            Assert.NotNull(openGammaFudgeContext);

            Assert.Throws<ComponentRegistrationException>(() => container.Register(Component.For<RemoteEngineContext>().UsingFactoryMethod<RemoteEngineContext>(() => null)));
        }
    }
}
