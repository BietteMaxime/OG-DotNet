// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumValuesDataAttribute.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Xunit.Extensions;

namespace OpenGamma.Xunit.Extensions
{
    public class EnumValuesDataAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo methodUnderTest, Type[] parameterTypes)
        {
            var type = parameterTypes.Single();
            if (!type.IsEnum)
            {
                throw new ArgumentException();
            }

            return from object value in Enum.GetValues(type) select new[] { value };
        }
    }
}
