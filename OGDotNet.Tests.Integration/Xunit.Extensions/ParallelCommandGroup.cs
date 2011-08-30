//-----------------------------------------------------------------------
// <copyright file="ParallelCommandGroup.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace OGDotNet.Tests.Integration.Xunit.Extensions
{
    internal class ParallelCommandGroup
    {
        public static IEnumerable<ITestCommand> WrapGroup(IEnumerable<ITestCommand> commands)
        {
            var group = new ParallelCommandGroup(commands);
            return group.WrappedCommands;
        }

        private readonly List<ITestCommand> _innerCommands = new List<ITestCommand>();
        private readonly IList<ParallelCommand> _wrappedCommands;

        private readonly Dictionary<ITestCommand, Task<MethodResult>> _tasks = new Dictionary<ITestCommand, Task<MethodResult>>();

        private ParallelCommandGroup(IEnumerable<ITestCommand> commands)
        {
            _innerCommands = commands.ToList();
            _wrappedCommands = _innerCommands.Select(c => new ParallelCommand(c, this)).ToList();
        }

        private IEnumerable<ITestCommand> WrappedCommands
        {
            get { return _wrappedCommands; }
        }

        readonly FieldInfo _methodInfoField = typeof(TestCommand).GetField("testMethod", BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic);

        private MethodResult Execute(ITestCommand inner)
        {
            if (! _tasks.Any())
            {
                foreach (var testCommand in _innerCommands)
                {
                    var command = (DelegatingTestCommand) testCommand;
                        
                    Func<MethodResult> action = 
                        () =>
                        {
                            var innerCommand = (IMethodInfo) _methodInfoField.GetValue(command.InnerCommand);
                            return new LifetimeCommand(command, innerCommand).Execute(null);
                        };

                    var task = new Task<MethodResult>(action, TaskCreationOptions.LongRunning);
                    _tasks.Add(testCommand, task);
                }

                StartAllTasks(_tasks.Values);
            }

            try
            {
                return _tasks[inner].Result;
            }
            catch (AggregateException e)
            {
                if (e.InnerExceptions.Count == 1)
                {
                    //This looks better in nunit
                    ExceptionUtility.RethrowWithNoStackTraceLoss(e.InnerException);
                    throw;
                }
                else
                {
                    throw;
                }
            }
        }

        private static void StartAllTasks(IEnumerable<Task<MethodResult>> values)
        {
            const int concurrentTasks = 4; //Seems to be fairly fast and not stress things too much

            var taskQueue = new ConcurrentQueue<Task>(values);
            for (int i = 0; i < concurrentTasks; i++)
            {
                StartOne(taskQueue);
            }
        }

        private static void StartOne(ConcurrentQueue<Task> taskQueue)
        {
            Task next;
            if (taskQueue.TryDequeue(out next))
            {
                next.ContinueWith(t => { var ignore = t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
                next.ContinueWith(t => StartOne(taskQueue));
                next.Start();
            }
        }

        private class ParallelCommand : DelegatingTestCommand, ITestCommand
        {
            private readonly ParallelCommandGroup _parallelCommandGroup;

            public ParallelCommand(ITestCommand inner, ParallelCommandGroup parallelCommandGroup)
                : base(inner)
            {
                _parallelCommandGroup = parallelCommandGroup;
            }

            public override MethodResult Execute(object testClass)
            {
                if (testClass != null)
                {
                    throw new ArgumentException("We've double created our fixtures", "testClass");
                }
                return _parallelCommandGroup.Execute(InnerCommand);
            }

            bool ITestCommand.ShouldCreateInstance
            {
                get
                {
                    return false;
                }
            }
        }
    }
}