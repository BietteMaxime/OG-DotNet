using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Castle.Core.Resource;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Castle.Windsor.Installer;
using OGDotNet.Model.Context;

namespace OGDotNet.WPFUtils.Windsor
{
    public class OGDotNetApp : Application
    {
        public static readonly DependencyProperty OGContextProperty = DependencyProperty.RegisterAttached("OGContext", typeof(RemoteEngineContext), typeof(Control), new FrameworkPropertyMetadata { Inherits = true });

        private readonly WindsorContainer _container;

        public RemoteEngineContext OGContext
        {
            get
            {
                return _container.Resolve<RemoteEngineContext>();
            }
        }

        public OGDotNetApp()
        {
            _container = new WindsorContainer(new XmlInterpreter(new ConfigResource("castle")));
            
           var defaultConfigurationStore = new DefaultConfigurationStore();
            FromAssembly.Containing<RemoteEngineContextFactory>().Install(_container, defaultConfigurationStore);
            _container.Register();

            //Give all of the windows the opportunity to pick up context
            Style windowStyle = new Style(typeof(Window));


            windowStyle.Setters.Add(new Setter(OGContextProperty,
                new Binding("OGContext") { Source = this }));
            
            FrameworkElement.StyleProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata
            {
                DefaultValue = windowStyle
            });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _container.Dispose();
        }
    }
}
