// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAvailableOutputs.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using Fudge.Serialization;

using OpenGamma.Fudge;

namespace OpenGamma.Engine.View.Helper
{
    [FudgeSurrogate(typeof(AvailableOutputsImplBuilder))]
    public interface IAvailableOutputs
    {
        ICollection<string> SecurityTypes { get; }
        ICollection<AvailableOutput> GetPositionOutputs(string securityType);
        ICollection<AvailableOutput> GetPortfolioNodeOutputs();
    }
}