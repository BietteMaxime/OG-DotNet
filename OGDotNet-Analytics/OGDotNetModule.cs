//-----------------------------------------------------------------------
// <copyright file="OGDotNetModule.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using OGDotNet.Model;
using OGDotNet.Model.Context;
using OGDotNet.Utils;

namespace OGDotNet
{
    public class OGDotNetModule : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<RemoteEngineContext>().UsingFactory((RemoteEngineContextFactory fac) => fac.CreateRemoteEngineContext()));

            container.Register(new ComponentRegistration<OpenGammaFudgeContext>());
            container.Register(new ComponentRegistration<LoggingUtils>());

            container.Resolve<LoggingUtils>().Init();
        }

        public class LoggingUtils : LoggingClassBase, IDisposable
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
