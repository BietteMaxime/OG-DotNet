//-----------------------------------------------------------------------
// <copyright file="FreezeDetector.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using Castle.Core.Logging;
using OGDotNet.Mappedtypes;
using OGDotNet.Utils;

namespace OGDotNet.WPFUtils.Windsor
{
    internal class FreezeDetector : LoggingClassBase
    {
        static readonly TimeSpan MaxBusy = TimeSpan.FromMinutes(0.5);
        static readonly TimeSpan SamplePeriod = TimeSpan.FromTicks(MaxBusy.Ticks / 10);

        private readonly Dispatcher _dispatcher;
        private readonly Thread _thread;
        private readonly DispatcherTimer _dispatcherTimer;

        private volatile bool _shuttingDown = false;
        private volatile bool _idleReached = false;

        private FreezeDetector(Dispatcher dispatcher, ILogger logger)
        {
            GlobalLogger = logger;
            _dispatcher = dispatcher;
            _thread = new Thread(Watchdog)
                          {
                              Name = typeof(FreezeDetector).FullName,
                              IsBackground = true
                          };
            _thread.Start();
            _dispatcherTimer = new DispatcherTimer(SamplePeriod, DispatcherPriority.ApplicationIdle, DispatcherInactive, dispatcher);
            _dispatcherTimer.Start();
            dispatcher.ShutdownStarted += DispatcherShuttingDown;
        }

        private void Watchdog()
        {
            while (! _shuttingDown)
            {
                _idleReached = false;
                Thread.Sleep(MaxBusy);
                if (! _idleReached)
                {
                    //Make sure we don't see our own post monitoring!
                    //TODO: should we sample more than once?
#pragma warning disable 612,618
                    var thread = _dispatcher.Thread;
                    thread.Suspend();
                    var timedOutTrace = new StackTrace(thread, false);
                    thread.Resume();
#pragma warning restore 612,618

                    var postedOperations = new BlockingCollection<DispatcherOperation>();
                    DispatcherHookEventHandler hooksOnOperationPosted = null;
                    const int maxOperationToLog = 100;
                    hooksOnOperationPosted = delegate(object sender, DispatcherHookEventArgs e)
                                                 {
                                                     if (postedOperations.Count < maxOperationToLog)
                                                     {
                                                         postedOperations.Add(e.Operation);
                                                     }
                                                     else
                                                     {
                                                         _dispatcher.Hooks.OperationPosted -= hooksOnOperationPosted;
                                                     }
                                                 };
                    _dispatcher.Hooks.OperationPosted += hooksOnOperationPosted;

                    var sb = new StringBuilder();
                    sb.AppendLine("Dispatcher is blocked:");
                    sb.AppendLine(timedOutTrace.ToString());
                    Thread.Sleep(SamplePeriod); //wait for some posts

                    var nameGetter = typeof(DispatcherOperation).GetProperty("Name", BindingFlags.Default  | BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
                    foreach (var operation in postedOperations)
                    {
                        sb.AppendLine(nameGetter.Invoke(operation, new object[] { }).ToString());
                    }
                    var message = sb.ToString();
                    Logger.Error(message);
                    throw new OpenGammaException(message); //Should be caught by general error handler
                }
            }
        }

        private void DispatcherShuttingDown(object sender, EventArgs e)
        {
            _shuttingDown = true;
        }
        private void DispatcherInactive(object sender, EventArgs e)
        {
            _idleReached = true;
        }

        public static void HookUp(Dispatcher dispatcher, ILogger logger)
        {
            new FreezeDetector(dispatcher, logger);
        }
    }
}