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
    public class UpdateAction
    {
        private readonly IEnumerable<Warning> _warnings;
        private readonly List<Action> _updateActions;
        private static readonly UpdateAction Empty = new UpdateAction(Enumerable.Empty<Action>(), Enumerable.Empty<Warning>());

        internal static UpdateAction Of(IEnumerable<UpdateAction> actions)
        {
            return actions.Aggregate(Empty, (a, b) => a.Concat(b));
        }

        internal UpdateAction(Action updateAction)
            : this(updateAction, Enumerable.Empty<Warning>())
        {
        }

        internal UpdateAction(Action updateAction, IEnumerable<Warning> warnings)
            : this(new List<Action> { updateAction }, warnings)
        {
        }

        private UpdateAction(IEnumerable<Action> updateActions, IEnumerable<Warning> warnings)
        {
            _warnings = warnings;
            _updateActions = updateActions.ToList();
        }

        public IEnumerable<Warning> Warnings
        {
            get { return _warnings; }
        }
        public void Execute()
        {
            foreach (var updateAction in _updateActions)
            {
                updateAction();
            }
        }

        private UpdateAction Concat(IEnumerable<Warning> w)
        {
            return new UpdateAction(_updateActions, Warnings.Concat(w));
        }

        private UpdateAction Concat(IEnumerable<Action> updateActions)
        {
            return new UpdateAction(_updateActions.Concat(updateActions), Warnings);
        }

        public UpdateAction Concat(UpdateAction r)
        {
            return Concat(r._warnings).Concat(r._updateActions);
        }
    }
}