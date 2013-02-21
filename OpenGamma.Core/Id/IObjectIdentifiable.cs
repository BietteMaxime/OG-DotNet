// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IObjectIdentifiable.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenGamma.Id
{
    public interface IObjectIdentifiable
    {
        ObjectId ObjectId { get; }
    }
}
