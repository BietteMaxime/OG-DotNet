using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Extensions;

namespace OGDotNet.Tests.Xunit.Extensions
{
    class EnumValuesDataAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo methodUnderTest, Type[] parameterTypes)
        {
            var type = parameterTypes.Single();
            if (! type.IsEnum)
            {
                throw new ArgumentException();
            }

            return from object value in Enum.GetValues(type) select new[]{value};
        }
    }
}
