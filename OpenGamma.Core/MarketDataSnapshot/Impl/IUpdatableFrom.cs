// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUpdatableFrom.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Model.Context.MarketDataSnapshot;

namespace OpenGamma.MarketDataSnapshot.Impl
{
    public interface IUpdatableFrom<T>
    {
        UpdateAction<T> PrepareUpdateFrom(T newObject);
        T Clone();
    }
}