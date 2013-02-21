// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotCalculatedSentinel.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Fudge.Serialization;
using OpenGamma.Fudge;

namespace OpenGamma.Engine.View.Cache
{
    [FudgeSurrogate(typeof(NotCalculatedSentinelBuilder))]
    public enum NotCalculatedSentinel
    {
        MissingInputs,
        EvaluationError
    }
}