//-----------------------------------------------------------------------
// <copyright file="ViewComputationResultModel.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.engine.View
{
    [FudgeSurrogate(typeof(ViewComputationResultModelBuilder))]
    public abstract class ViewComputationResultModel
    {
        public abstract UniqueIdentifier ViewProcessId { get; }

        public abstract UniqueIdentifier ViewCycleId { get; }
    }
}