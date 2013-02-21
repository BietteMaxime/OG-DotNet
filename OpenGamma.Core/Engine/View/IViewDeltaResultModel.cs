// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IViewDeltaResultModel.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge.Serialization;

using OpenGamma.Fudge.ViewResultModel;

namespace OpenGamma.Engine.View
{
    [FudgeSurrogate(typeof(InMemoryViewDeltaResultModelBuilder))]
    public interface IViewDeltaResultModel : IViewResultModel
    {
        DateTimeOffset PreviousResultTimestamp { get; }
    }
}