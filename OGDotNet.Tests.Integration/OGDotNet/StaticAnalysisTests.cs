//-----------------------------------------------------------------------
// <copyright file="StaticAnalysisTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.Engine;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Tests.Integration.Xunit.Extensions;
using OGDotNet.Tests.Xunit.Extensions;
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet
{
    public class StaticAnalysisTests
    {
        public static IEnumerable<Type> Types
        {
            get
            {
                var mappedTypes = typeof(UniqueIdentifier).Assembly.GetTypes().ToList();
                Assert.NotEmpty(mappedTypes);
                return mappedTypes;
            }
        }

        [Theory]
        [TypedPropertyData("Types")]
        public void ConsistentUniqueIdentifierPropertyName(Type mappedType)
        {
            foreach (var propertyInfo in mappedType.GetProperties().Where(p => p.PropertyType == typeof(UniqueIdentifier)))
            {
                switch (propertyInfo.Name)
                {
                    case "Identifier":
                    case "Id":
                    case "UniqueIdentifier":
                        throw new Exception(string.Format("{0} has property {1}, use UniqeId instead (And {2})", mappedType.Name, propertyInfo.Name, typeof(IUniqueIdentifiable).Name));
                }
            }
        }

        [Theory]
        [TypedPropertyData("Types")]
        public void ExplicitImplementationofIUniqueIdentifiable(Type mappedType)
        {
            if (
                mappedType.GetProperties().Where(p => p.PropertyType == typeof(UniqueIdentifier) && p.Name == "UniqueId").Any()
                && !typeof(IUniqueIdentifiable).IsAssignableFrom(mappedType))
            {
                if (mappedType != typeof(ComputationTarget)
                    &&
                    mappedType != typeof(MarketDataValueSpecification))
                {
                    throw new Exception(string.Format("{0} duck types as {1} but doesn't implement it",
                                                      mappedType.Name, typeof(IUniqueIdentifiable).Name));
                }
            }
        }

        [Theory]
        [TypedPropertyData("Types")]
        public void InterfacesNamedAsInterfaces(Type mappedType)
        {
            if (mappedType.IsInterface)
            {
                var name = mappedType.Name;
                if (name[0] != 'I' || !char.IsUpper(name[1]))
                {
                    Assert.Equal(name, "ISomething");
                }
            }
        }

        [Xunit.Extensions.Fact]
        public void NamespaceCasingConsistent()
        {
            var nameSpaces = new HashSet<string>();

            foreach (var type in Types)
            {
                //TODO add parents
                nameSpaces.Add(type.Namespace);
            }

            foreach (var g in nameSpaces.ToLookup(s => s.ToUpperInvariant()))
            {
                if (g.Count() > 1)
                {
                    throw new Exception(string.Format("Found many casings {0}: {1}", g.Key, string.Join(",", g)));
                }
            }
        }
    }
}
