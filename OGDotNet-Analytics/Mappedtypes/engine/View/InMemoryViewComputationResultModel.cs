//-----------------------------------------------------------------------
// <copyright file="InMemoryViewComputationResultModel.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using OGDotNet.Mappedtypes.Engine.Value;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Engine.View
{
    public class InMemoryViewComputationResultModel : InMemoryViewResultModelBase, IViewComputationResultModel
    {
        private readonly List<ComputedValue> _allLiveData;

        public InMemoryViewComputationResultModel(UniqueIdentifier viewProcessId, UniqueIdentifier viewCycleId, DateTimeOffset inputDataTimestamp, DateTimeOffset resultTimestamp, IDictionary<string, ViewCalculationResultModel> configurationMap, List<ComputedValue> allLiveData, TimeSpan calculationDuration)
            : base(viewProcessId, viewCycleId, inputDataTimestamp, resultTimestamp, configurationMap, calculationDuration)
        {
            _allLiveData = allLiveData;
        }

        public IEnumerable<ComputedValue> AllLiveData
        {
            get { return _allLiveData; }
        }
    }
}