// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AggregatingChangeListener.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace OpenGamma.Core.Change
{
    public class AggregatingChangeListener : IChangeListener
    {
        private readonly object _lock = new object();
        private readonly List<ChangeEvent> _events = new List<ChangeEvent>();

        public void EntityChanged(ChangeEvent changeEvent)
        {
            lock (_lock)
            {
                _events.Add(changeEvent);
            }
        }

        public List<ChangeEvent> GetAndClearEvents()
        {
            lock (_lock)
            {
                var ret = new List<ChangeEvent>(_events);
                _events.Clear();
                return ret;
            }
        }
    }
}