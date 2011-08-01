//-----------------------------------------------------------------------
// <copyright file="ICompiledFunctionDefinition.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

namespace OGDotNet.Mappedtypes.Engine.function
{
    public interface ICompiledFunctionDefinition
    {
        ComputationTargetType TargetType { get; }
        IFunctionDefinition FunctionDefinition { get; }
    }
}
