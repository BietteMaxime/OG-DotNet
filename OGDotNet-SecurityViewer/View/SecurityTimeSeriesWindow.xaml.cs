using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OGDotNet.Mappedtypes.Core.Security;
using OGDotNet.Mappedtypes.math.curve;
using OGDotNet.Model.Context;
using OGDotNet.Model.Resources;

namespace OGDotNet.SecurityViewer.View
{
    /// <summary>
    /// Interaction logic for SecurityTimeSeriesWindow.xaml
    /// </summary>
    public partial class SecurityTimeSeriesWindow : Window
    {
        private RemoteHistoricalDataSource _dataSource;

        public SecurityTimeSeriesWindow()
        {
            InitializeComponent();
        }

        public RemoteEngineContext Context
        {
            set
            {
                if (_dataSource != null)
                    throw new NotImplementedException("Can't handle context changing yet");
                _dataSource = value.HistoricalDataSource;
                Update();
            }
        }

        private Security Security
        {
            get { return (Security) DataContext; }
        }
        private void Window_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Update();
        }

        private void Update()
        {
            if (_dataSource == null || Security == null)
                return;

            var sb = new StringBuilder();
            foreach (var propertyInfo in Security.GetType().GetProperties())
            {
                sb.AppendFormat(string.Format("{0}: {1}", propertyInfo.Name, propertyInfo.GetGetMethod().Invoke(Security, null)));
                sb.Append(Environment.NewLine);
            }
            detailsBlock.Text = sb.ToString();

            var timeSeries = _dataSource.GetHistoricalData(Security.Identifiers).Item2;
            var tuples = timeSeries.Values;
            //TODO this is a hack, this is not really a curve
            var timeSeriesAsCurve = new InterpolatedDoublesCurve(string.Format("Time Series for {0}", Security.Name),
                                                                 tuples.Select(
                                                                     t => (t.Item1 - DateTimeOffset.FromFileTime(0)).TotalMilliseconds
                                                                     ).ToArray(),
                                                                 tuples.Select(
                                                                     t => t.Item2
                                                                     ).ToArray()
                );
            curveControl.DataContext = timeSeriesAsCurve;
        }
    }
}
