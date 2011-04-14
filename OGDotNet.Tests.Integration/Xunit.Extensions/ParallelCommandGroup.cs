//-----------------------------------------------------------------------
// <copyright file="ParallelCommandGroup.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        private readonly Dictionary<ITestCommand, Func<MethodResult>> _actions = new Dictionary<ITestCommand, Func<MethodResult>>();
        private readonly Dictionary<ITestCommand, IAsyncResult> _results = new Dictionary<ITestCommand, IAsyncResult>();

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
            if (! _actions.Any())
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

                    _actions.Add(testCommand, action);
                    _results.Add(testCommand, action.BeginInvoke(null, null));
                }
            }

            return _actions[inner].EndInvoke(_results[inner]);
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