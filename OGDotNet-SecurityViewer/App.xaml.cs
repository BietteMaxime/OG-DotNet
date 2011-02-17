using System.Windows;
using Castle.Core.Resource;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using OGDotNet.Model.Context;
using OGDotNet.SecurityViewer.View;

namespace OGDotNet_SecurityViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly WindsorContainer _container;
        private SecurityWindow _window;

        public App()
        {
            _container = new WindsorContainer(new XmlInterpreter(new ConfigResource("castle")));
            _container.Register(new ComponentRegistration<SecurityWindow>());

            var defaultConfigurationStore = new DefaultConfigurationStore();
            Castle.Windsor.Installer.FromAssembly.Containing<RemoteEngineContext>().Install(_container, defaultConfigurationStore);
            _container.Register();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _container.Release(_window);
            _container.Dispose();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _window = _container.Resolve<SecurityWindow>();
            _window.Show();
        }
    }
}
