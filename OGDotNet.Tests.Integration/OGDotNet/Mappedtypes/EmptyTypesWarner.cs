//-----------------------------------------------------------------------
// <copyright file="EmptyTypesWarner.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Mappedtypes
{
    public class EmptyTypesWarner
    {
        public static IEnumerable<Type> MappedTypes
        {
            get
            {
                var mappedTypes = typeof(UniqueIdentifier).Assembly.GetTypes().Where(t => t.FullName.StartsWith("OGDotNet.Mappedtypes")).ToList();
                Assert.NotEmpty(mappedTypes);
                return mappedTypes;
            }
        }

        private static readonly int LeastUseful = typeof(object).GetProperties().Count() + typeof(object).GetMethods().Count();

        [Theory]
        [TypedPropertyData("MappedTypes")]
        public void TypesArentUseless(Type mappedType)
        {
            if (mappedType.IsInterface)
                return;
            Assert.True(GetUsefuleness(mappedType)  > LeastUseful, string.Format("Useless mapped type {0}", mappedType.FullName));
        }

        private static int GetUsefuleness(Type mappedType)
        {
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | (mappedType == typeof(object) ? 0 : BindingFlags.Static);
            
            return mappedType.GetProperties(bindingFlags).Count() + mappedType.GetMethods(bindingFlags).Count();
        }
    }
}
