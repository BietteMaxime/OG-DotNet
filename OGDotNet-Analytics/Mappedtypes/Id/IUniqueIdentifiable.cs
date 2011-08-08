//-----------------------------------------------------------------------
// <copyright file="IUniqueIdentifiable.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
namespace OGDotNet.Mappedtypes.Id
{
    public interface IUniqueIdentifiable
    {
        UniqueId UniqueId { get; }
    }
}
