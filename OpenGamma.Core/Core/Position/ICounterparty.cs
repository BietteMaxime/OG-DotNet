// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICounterparty.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Id;

namespace OpenGamma.Core.Position
{
    public interface ICounterparty
    {
        ExternalId ExternalId { get; }
    }
}
