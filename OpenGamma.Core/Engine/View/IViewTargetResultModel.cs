// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IViewTargetResultModel.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using OpenGamma.Engine.Value;

namespace OpenGamma.Engine.View
{
    public interface IViewTargetResultModel
    {
        IEnumerable<string> CalculationConfigurationNames { get; }
        IDictionary<string, ComputedValue> GetValues(string calcConfigurationName);
    }
}