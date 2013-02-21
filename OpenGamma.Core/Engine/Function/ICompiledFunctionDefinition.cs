// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICompiledFunctionDefinition.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Engine.Target;

namespace OpenGamma.Engine.Function
{
    public interface ICompiledFunctionDefinition
    {
        ComputationTargetType TargetType { get; }
        IFunctionDefinition FunctionDefinition { get; }
    }
}
