//-----------------------------------------------------------------------
// <copyright file="IFunctionDefinition.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
namespace OGDotNet.Mappedtypes.engine.function
{
    public interface IFunctionDefinition
    {
        string UniqueId { get; }
        string ShortName { get; }
    }
}