//-----------------------------------------------------------------------
// <copyright file="ViewClientState.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using Fudge.Serialization;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.util.PublicAPI
{
    public enum ViewClientState
    {
        Started,
        Stopped,
        Paused,
        Terminated
    }
}