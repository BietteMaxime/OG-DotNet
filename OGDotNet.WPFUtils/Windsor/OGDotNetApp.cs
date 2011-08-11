//-----------------------------------------------------------------------
// <copyright file="OGDotNetApp.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Castle.Core.Resource;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Castle.Windsor.Installer;
using OGDotNet.Model.Context;
using Configuration = Castle.Windsor.Installer.Configuration;

namespace OGDotNet.WPFUtils.Windsor
{
    public class OGDotNetApp : Application
    {
        public static readonly DependencyProperty OGContextProperty = DependencyProperty.RegisterAttached("OGContext", typeof(RemoteEngineContext), typeof(Control), new FrameworkPropertyMetadata { Inherits = true });
        public static readonly DependencyProperty OGContextFactoryProperty = DependencyProperty.RegisterAttached("OGContextFactory", typeof(RemoteEngineContextFactory), typeof(Control), new FrameworkPropertyMetadata { Inherits = true });

        private readonly WindsorContainer _container;

        public RemoteEngineContext OGContext
        {
            get
            {
                return _container.Resolve<RemoteEngineContext>();
            }
        }
        public RemoteEngineContextFactory OGContextFactory
        {
            get
            {
                return _container.Resolve<RemoteEngineContextFactory>();
            }
        }

        public OGDotNetApp()
        {
            //Can't read default config directly if we're untrusted http://social.msdn.microsoft.com/Forums/en-US/clr/thread/1e14f665-10a3-426b-a75d-4e66354c5522
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var section = config.Sections["castle"];
            var configXml = section.SectionInformation.GetRawXml();
            var resource = new StaticContentResource(configXml);
            var xmlInterpreter = new XmlInterpreter(resource);
            _container = new WindsorContainer(xmlInterpreter);
            
            FromAssembly.Containing<RemoteEngineContextFactory>().Install(_container, new DefaultConfigurationStore());

            _container.Register();

            //Give all of the windows the opportunity to pick up context
            Style windowStyle = new Style(typeof(Window));

            windowStyle.Setters.Add(new Setter(OGContextProperty, new Binding("OGContext") { Source = this }));
            windowStyle.Setters.Add(new Setter(OGContextFactoryProperty, new Binding("OGContextFactory") { Source = this }));

            FrameworkElement.StyleProperty.OverrideMetadata(typeof(Window), 
                new FrameworkPropertyMetadata { DefaultValue = windowStyle }
                );
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _container.Dispose();
        }
    }
}
