//-----------------------------------------------------------------------
// <copyright file="IStreamingFudgeBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Model;

namespace OGDotNet.Builders.Streaming
{
    interface IStreamingFudgeBuilder
    {
        T Deserialize<T>(OpenGammaFudgeContext context, IFudgeStreamReader stream, SerializationTypeMap typeMap);
        Type Type { get; }
    }
}