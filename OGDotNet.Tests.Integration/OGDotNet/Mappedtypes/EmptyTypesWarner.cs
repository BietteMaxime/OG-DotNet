using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using Xunit;
using Xunit.Extensions;

namespace OGDotNet.Tests.Integration.OGDotNet.Mappedtypes
{
    public class EmptyTypesWarner
    {
        public static IEnumerable<Type> MappedTypes
        {
            get
            {
                var mappedTypes = typeof (UniqueIdentifier).Assembly.GetTypes().Where(t => t.FullName.StartsWith("OGDotNet.Mappedtypes")).ToList();
                Assert.NotEmpty(mappedTypes);
                return mappedTypes;
            }
        }

        [Theory(Skip = "Known fault - [TODO add JIRA ID]")]
        [TypedPropertyData("MappedTypes")]
        public void TypesArentUseless(Type mappedType)
        {
            Assert.True(GetUsefuleness(mappedType)  > GetUsefuleness(typeof(object)), string.Format("Useless mapped type {0}", mappedType.FullName));
        }

        private static int GetUsefuleness(Type mappedType)
        {
            return mappedType.GetProperties().Count() + mappedType.GetMethods().Count();
        }
    }
}
