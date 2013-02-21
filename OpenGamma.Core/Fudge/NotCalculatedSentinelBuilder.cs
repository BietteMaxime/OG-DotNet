// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotCalculatedSentinelBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Fudge;
using OpenGamma.Engine.View.Cache;

namespace OpenGamma.Fudge
{
    internal class NotCalculatedSentinelBuilder : EnumBuilder<NotCalculatedSentinel>
    {
        public NotCalculatedSentinelBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }
    }
}