//-----------------------------------------------------------------------
// <copyright file="IDeltaComparer.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
namespace OGDotNet.Mappedtypes.Engine.View
{
    public interface IDeltaComparer<in T>
    {
        bool IsDelta(T previousValue, T newValue);
    }
}