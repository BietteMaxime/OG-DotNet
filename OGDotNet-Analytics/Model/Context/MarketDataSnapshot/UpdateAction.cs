//-----------------------------------------------------------------------
// <copyright file="UpdateAction.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Model.Context.MarketDataSnapshot.Warnings;

namespace OGDotNet.Model.Context.MarketDataSnapshot
{
    public class UpdateAction<T>
    {
        private readonly List<Warning> _warnings;
        private readonly List<Action<T>> _updateActions;
        private static readonly UpdateAction<T> Empty = new UpdateAction<T>(Enumerable.Empty<Action<T>>(), Enumerable.Empty<Warning>());

        internal static UpdateAction<T> Of(IEnumerable<UpdateAction<T>> actions)
        {
            return actions.Aggregate(Empty, (a, b) => a.Concat(b));
        }

        internal UpdateAction(Action<T> updateAction)
            : this(updateAction, Enumerable.Empty<Warning>())
        {
        }

        internal UpdateAction(Action<T> updateAction, IEnumerable<Warning> warnings)
            : this(new List<Action<T>> { updateAction }, warnings)
        {
        }

        private UpdateAction(IEnumerable<Action<T>> updateActions, IEnumerable<Warning> warnings)
        {
            _warnings = warnings.ToList();
            _updateActions = updateActions.ToList();
        }

        public IEnumerable<Warning> Warnings
        {
            get { return _warnings; }
        }
        public void Execute(T target)
        {
            foreach (var updateAction in _updateActions)
            {
                updateAction(target);
            }
        }

        private UpdateAction<T> Concat(IEnumerable<Warning> w)
        {
            return new UpdateAction<T>(_updateActions, Warnings.Concat(w));
        }

        private UpdateAction<T> Concat(IEnumerable<Action<T>> updateActions)
        {
            return new UpdateAction<T>(_updateActions.Concat(updateActions), Warnings);
        }

        public UpdateAction<T> Concat(UpdateAction<T> r)
        {
            return Concat(r._warnings).Concat(r._updateActions);
        }

        public UpdateAction<TOuter> Wrap<TOuter>(Func<TOuter, T> projecter)
        {
            return new UpdateAction<TOuter>(_updateActions.Select<Action<T>, Action<TOuter>>(a => (o => a(projecter(o)))), Warnings);
        }

        private static Action<T> Wrap<TInner>(Action<TInner> action, Func<T, TInner> projecter)
        {
            return t => action(projecter(t));
        }

        private static UpdateAction<T> Wrap<TInner>(UpdateAction<TInner> r, Func<T, TInner> projecter)
        {
            return new UpdateAction<T>(r._updateActions.Select(a => Wrap(a, projecter)), r.Warnings);
        }
    }
}