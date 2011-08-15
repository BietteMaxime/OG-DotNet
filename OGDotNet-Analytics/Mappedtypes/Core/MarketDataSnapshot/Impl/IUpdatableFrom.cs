//-----------------------------------------------------------------------
// <copyright file="IUpdatableFrom.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Model.Context.MarketDataSnapshot;

namespace OGDotNet.Mappedtypes.Core.MarketDataSnapshot.Impl
{
    public interface IUpdatableFrom<T>
    {
        UpdateAction<T> PrepareUpdateFrom(T newObject);
    }
}