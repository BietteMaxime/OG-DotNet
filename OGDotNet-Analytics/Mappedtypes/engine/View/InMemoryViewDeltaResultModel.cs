//-----------------------------------------------------------------------
// <copyright file="InMemoryViewDeltaResultModel.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.engine.View
{
    public class InMemoryViewDeltaResultModel : InMemoryViewResultModelBase, ViewDeltaResultModel
    {
        private readonly DateTimeOffset _previousResultTimestamp;

        public InMemoryViewDeltaResultModel(UniqueIdentifier viewProcessId, UniqueIdentifier viewCycleId, DateTimeOffset inputDataTimestamp, DateTimeOffset resultTimestamp, IDictionary<string, ViewCalculationResultModel> configurationMap, DateTimeOffset previousResultTimestamp) : base(viewProcessId, viewCycleId, inputDataTimestamp, resultTimestamp, configurationMap)
        {
            _previousResultTimestamp = previousResultTimestamp;
        }

        public DateTimeOffset PreviousResultTimestamp
        {
            get { return _previousResultTimestamp; }
        }
    }
}