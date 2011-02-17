using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using OGDotNet.Model.Context;
using OGDotNet.Model.Resources;

namespace OGDotNet
{
    public class OGDotNetModule : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<RemoteEngineContext>().UsingFactory((RemoteEngineContextFactory fac) => fac.CreateRemoteEngineContext()));

            container.Register(Component.For<RemoteSecurityMaster>().UsingFactory((RemoteEngineContext context) => context.SecurityMaster));
        }
    }
}
