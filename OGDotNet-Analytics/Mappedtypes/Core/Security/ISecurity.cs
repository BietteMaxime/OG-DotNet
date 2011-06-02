//-----------------------------------------------------------------------
// <copyright file="ISecurity.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Core.Security
{
    /// <summary>
    /// TODO DOTNET-5: kill these with the .proto replacement
    /// </summary>
    public interface ISecurity : IUniqueIdentifiable
    {
        string Name { get; }

        string SecurityType { get; }

        IdentifierBundle Identifiers { get; }
    }
}