//-----------------------------------------------------------------------
// <copyright file="ICompiledViewDefinitionWithGraphs.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
namespace OGDotNet.Mappedtypes.engine.View.compilation
{
    public interface ICompiledViewDefinitionWithGraphs : ICompiledViewDefinition
    {
        //TODO [PLAT-883] Currently no dependency graph browsing is possible through the interface, and therefore remotely.
    }
}