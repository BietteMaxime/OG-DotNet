//-----------------------------------------------------------------------
// <copyright file="ViewComputationResultModel.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Builders.ViewResultModel;
using OGDotNet.Mappedtypes.engine.value;

namespace OGDotNet.Mappedtypes.engine.View
{
    [FudgeSurrogate(typeof(InMemoryViewComputationResultModelBuilder))]

    [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1302:InterfaceNamesMustBeginWithI", Justification = "Name used for fudge mapping")]
    // ReSharper disable InconsistentNaming
    public interface ViewComputationResultModel : IViewResultModel
    // ReSharper restore InconsistentNaming
    {
        IEnumerable<ComputedValue> AllLiveData { get; }
    }
}