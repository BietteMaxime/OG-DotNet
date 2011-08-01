// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IViewCycleExecutionSequence.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace OGDotNet.Mappedtypes.Engine.View.Execution
{
    public interface IViewCycleExecutionSequence
    {
        bool IsEmpty { get; }
        ViewCycleExecutionOptions Next { get; }
    }
}