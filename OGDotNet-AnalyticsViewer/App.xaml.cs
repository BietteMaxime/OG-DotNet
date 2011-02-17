using System.Windows;
using Castle.Core.Resource;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using OGDotNet.AnalyticsViewer.View;
using OGDotNet.Model.Context;

namespace OGDotNet.AnalyticsViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly WindsorContainer _container;
        private MainWindow _window;

        public App()
        {
            _container = new WindsorContainer(new XmlInterpreter(new ConfigResource("castle")));
            _container.Register(new ComponentRegistration<MainWindow>());

           var defaultConfigurationStore = new DefaultConfigurationStore();
            Castle.Windsor.Installer.FromAssembly.Containing<RemoteEngineContextFactory>().Install(_container, defaultConfigurationStore);
            _container.Register();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _container.Release(_window);
            _container.Dispose();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _window = _container.Resolve<MainWindow>();
            _window.Show();
        }
    }
}
