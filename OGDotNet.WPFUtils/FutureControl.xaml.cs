// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FutureControl.xaml.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OGDotNet.WPFUtils
{
    /// <summary>
    /// Interaction logic for FutureControl.xaml
    /// </summary>
    public partial class FutureControl : UserControl
    {
        public static DependencyProperty LoadingVisibilityProperty = DependencyProperty.Register("LoadingVisibility", typeof(Visibility), typeof(FutureControl), new PropertyMetadata(Visibility.Visible));
        public static DependencyProperty ErrorVisibilityProperty = DependencyProperty.Register("ErrorVisibility", typeof(Visibility), typeof(FutureControl), new PropertyMetadata(Visibility.Hidden));
        public static DependencyProperty ErrorTextProperty = DependencyProperty.Register("ErrorText", typeof(string), typeof(FutureControl));
        public static DependencyProperty ErrorDetailTextProperty = DependencyProperty.Register("ErrorDetailText", typeof(string), typeof(FutureControl));

        private long _taskId;
        public FutureControl()
        {
            InitializeComponent();
        }

        public bool AllowStaleValues { get; set; }
        public Visibility LoadingVisibility
        {
            get
            {
                return (Visibility)GetValue(LoadingVisibilityProperty);
            }

            set
            {
                SetValue(LoadingVisibilityProperty, value);
            }
        }
        public Visibility ErrorVisibility
        {
            get
            {
                return (Visibility)GetValue(ErrorVisibilityProperty);
            }

            set
            {
                SetValue(ErrorVisibilityProperty, value);
            }
        }
        public string ErrorText
        {
            get
            {
                return (string)GetValue(ErrorTextProperty);
            }

            set
            {
                SetValue(ErrorTextProperty, value);
            }
        }
        public string ErrorDetailText
        {
            get
            {
                return (string)GetValue(ErrorDetailTextProperty);
            }

            set
            {
                SetValue(ErrorDetailTextProperty, value);
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var content = Content as FrameworkElement;
            var dataContext = DataContext as Task; // This is really Task<?> but I haven't bothered to express that
            if (content != null && dataContext != null)
            {
                var taskId = Interlocked.Increment(ref _taskId);

                LoadingVisibility = Visibility.Visible;
                content.IsEnabled = false;
                if (!AllowStaleValues)
                {
                    content.DataContext = null;
                }

                dataContext.ContinueWith(t =>
                {
                    Dispatcher.BeginInvoke((Action)delegate
                    {
                        if (Interlocked.Read(ref _taskId) != taskId)
                            return;

                        object result;
                        if (t.Exception != null)
                        {
                            result = null;
                            ErrorVisibility = Visibility.Visible;
                            string detailMessage;
                            ErrorText = GetErrorText(t.Exception, out detailMessage);
                            ErrorDetailText = detailMessage;
                        }
                        else
                        {
                            ErrorVisibility = Visibility.Hidden;
                            result = t.GetType().GetProperty("Result").GetGetMethod().Invoke(dataContext, new object[] { });
                        }

                        content.DataContext = result;
                        content.IsEnabled = true;
                        LoadingVisibility = Visibility.Hidden;
                    });
                }

                    );
                if (dataContext.Status == TaskStatus.Created)
                {
                    dataContext.Start();
                }
            }
        }

        private static string GetErrorText(Exception e, out string detailMessage)
        {
            var agg = e as AggregateException;
            if (agg != null && agg.InnerExceptions.Count == 1)
            {
                return GetErrorText(agg.InnerExceptions[0], out detailMessage);
            }

            detailMessage = e.ToString();
            return e.Message;
        }
    }
}
