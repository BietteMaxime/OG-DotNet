//-----------------------------------------------------------------------
// <copyright file="SerializerExtensions.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Builders
{
    public static class SerializerExtensions
    {
        public static void WriteTypeHeader(this IFudgeSerializer s, IAppendingFudgeFieldContainer a, Type type)
        {
            //Unlike the java library this functionality is not exposed, so we have to duplicate it
            var map = (IFudgeTypeMappingStrategy)s.Context.GetProperty(ContextProperties.TypeMappingStrategyProperty);
            a.Add(0, map.GetName(type));
        }
    }
}
