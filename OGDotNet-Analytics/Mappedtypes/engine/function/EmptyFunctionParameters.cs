//-----------------------------------------------------------------------
// <copyright file="EmptyFunctionParameters.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using Fudge.Serialization;
using OGDotNet.Builders;

namespace OGDotNet.Mappedtypes.Engine.Function
{
    [FudgeSurrogate(typeof(EmptyFunctionParametersBuilder))]
    public class EmptyFunctionParameters : IFunctionParameters
    {
    }
}
