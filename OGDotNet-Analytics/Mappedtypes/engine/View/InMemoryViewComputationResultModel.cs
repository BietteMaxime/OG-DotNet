//-----------------------------------------------------------------------
// <copyright file="InMemoryViewComputationResultModel.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.engine.View
{
    public class InMemoryViewComputationResultModel : InMemoryViewResultModelBase, ViewComputationResultModel
    {
        private readonly List<ComputedValue> _allLiveData;

        public InMemoryViewComputationResultModel(UniqueIdentifier viewProcessId, UniqueIdentifier viewCycleId, DateTimeOffset inputDataTimestamp, DateTimeOffset resultTimestamp, IDictionary<string, ViewCalculationResultModel> configurationMap, List<ComputedValue> allLiveData)
            : base(viewProcessId, viewCycleId, inputDataTimestamp, resultTimestamp, configurationMap)
        {
            _allLiveData = allLiveData;
        }

        public IEnumerable<ComputedValue> AllLiveData
        {
            get { return _allLiveData; }
        }
    }
}