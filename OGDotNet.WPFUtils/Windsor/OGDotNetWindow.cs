using System;
using System.Windows;
using OGDotNet.Model.Context;

namespace OGDotNet.WPFUtils.Windsor
{
    public class OGDotNetWindow : Window
    {
        protected RemoteEngineContext OGContext
        {
            get
            {
                return (RemoteEngineContext) Dispatcher.Invoke((Func<object>) ( ()=> GetValue(OGDotNetApp.OGContextProperty)));
            }
        }
    }
}