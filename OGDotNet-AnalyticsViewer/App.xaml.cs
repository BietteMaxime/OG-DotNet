using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Castle.Core.Resource;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Castle.Windsor.Installer;
using OGDotNet.AnalyticsViewer.View;
using OGDotNet.Model.Context;
using OGDotNet.WPFUtils.Windsor;

namespace OGDotNet.AnalyticsViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : OGDotNetApp
    {
    }
}

