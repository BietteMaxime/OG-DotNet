//-----------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Windows;
using Castle.Core.Resource;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using OGDotNet.Model.Context;
using OGDotNet.SecurityViewer.View;
using OGDotNet.WPFUtils.Windsor;

namespace OGDotNet_SecurityViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : OGDotNetApp
    {
    }
}
