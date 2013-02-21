// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IViewComputationResultModel.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using Fudge.Serialization;

using OpenGamma.Engine.Value;
using OpenGamma.Fudge.ViewResultModel;

namespace OpenGamma.Engine.View
{
    [FudgeSurrogate(typeof(InMemoryViewComputationResultModelBuilder))]
    public interface IViewComputationResultModel : IViewResultModel
    {
        IEnumerable<ComputedValue> AllLiveData { get; }
    }
}