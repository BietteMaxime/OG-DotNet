using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using OGDotNet.Mappedtypes.Core.Security;
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
            }
        }

        private Security Security
        {
            get { return (Security) DataContext; }
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = Security.Name;
            UpdateDetailsBlock();
            BeginInvokeOnIdle(delegate
                                  {
                                      UpdateChart();
                                      BeginInvokeOnIdle(() => Cursor = null);
                                  });
        }

        private void UpdateDetailsBlock()
        {
            var sb = new StringBuilder();
            foreach (var propertyInfo in Security.GetType().GetProperties())
            {
                sb.AppendFormat(string.Format("{0}: {1}", propertyInfo.Name, propertyInfo.GetGetMethod().Invoke(Security, null)));
                sb.Append(Environment.NewLine);
            }
            detailsBlock.Text = sb.ToString();
        }

        private void UpdateChart()
        {
            var historicalData = _dataSource.GetHistoricalData(Security.Identifiers);
            var timeSeries = historicalData.Item2;

            //NOTE: the chart understands DateTime, but not DateTime Offset
            var tuples = timeSeries.Values.Select(t => Tuple.Create(t.Item1.LocalDateTime, t.Item2)).ToList();

            lineSeries.ItemsSource = tuples;
            lineSeries.Title = historicalData.Item1.ToString();
        }


        private void BeginInvokeOnIdle(Action act)
        {
            Dispatcher.BeginInvoke(act, DispatcherPriority.ContextIdle);
        }
    }
}
