// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenGammaModule.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;

using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

using OpenGamma.Fudge;
using OpenGamma.Model;
using OpenGamma.Model.Context;
using OpenGamma.Util;

namespace OpenGamma
{
    public class OpenGammaModule : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<RemoteEngineContext>().UsingFactory((RemoteEngineContextFactory fac) => fac.CreateRemoteEngineContext()));

            container.Register(new ComponentRegistration<OpenGammaFudgeContext>());
            container.Register(new ComponentRegistration<LoggingUtils>());

            container.Resolve<LoggingUtils>().Init();
        }

        public sealed class LoggingUtils : LoggingClassBase, IDisposable
        {
            public LoggingUtils()
            {
                TaskScheduler.UnobservedTaskException += UnobservedTaskException;
            }

            public void Init()
            {
            }

            private void UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
            {
                Logger.Fatal("Unobserved task exception", e.Exception);
            }

            public void Dispose()
            {
                TaskScheduler.UnobservedTaskException -= UnobservedTaskException;
            }
        }
    }
}
