// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OGDotNetWindow.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Windows;

using OpenGamma.Model.Context;

namespace OGDotNet.WPFUtils.Windsor
{
    public class OGDotNetWindow : Window
    {
        public RemoteEngineContext OGContext
        {
            get
            {
                return (RemoteEngineContext)Dispatcher.Invoke((Func<object>)(() => GetValue(OGDotNetApp.OGContextProperty)));
            }
        }
        protected RemoteEngineContextFactory OGContextFactory
        {
            get
            {
                return (RemoteEngineContextFactory)Dispatcher.Invoke((Func<object>)(() => GetValue(OGDotNetApp.OGContextFactoryProperty)));
            }
        }
    }
}