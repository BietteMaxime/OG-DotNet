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
using OGDotNet.Mappedtypes.Analytics.Financial.Interestrate;
using OGDotNet.Mappedtypes.Analytics.Financial.Model.FiniteDifference;
using OGDotNet.Mappedtypes.Analytics.Financial.Model.Interestrate;
using OGDotNet.Mappedtypes.Analytics.Financial.Model.Interestrate.Curve;
using OGDotNet.Mappedtypes.Analytics.Financial.Model.Option.Definition;
using OGDotNet.Mappedtypes.Engine.MarketData.Spec;
using OGDotNet.Mappedtypes.Engine.View.Listener;
using OGDotNet.Mappedtypes.Financial.Analytics;
using OGDotNet.Mappedtypes.Financial.Forex.Method;
using OGDotNet.Mappedtypes.Financial.Greeks;
using OGDotNet.Mappedtypes.Financial.InterestRate;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Tests.Xunit.Extensions;
using Xunit;
using Xunit.Extensions;
using OGDotNet.Mappedtypes.Analytics.Financial.Model.Volatility.Surface;

namespace OGDotNet.Tests.OGDotNet.Mappedtypes
{
    public class EmptyTypesWarner
    {
        public static IEnumerable<Type> MappedTypes
        {
            get
            {
                var mappedTypes = typeof(UniqueId).Assembly.GetTypes().Where(t => t.FullName.StartsWith("OGDotNet.Mappedtypes")).ToList();
                Assert.NotEmpty(mappedTypes);
                return mappedTypes;
            }
        }

        private static readonly int LeastUseful = typeof(object).GetProperties().Count() + typeof(object).GetMethods().Count();

        public static readonly HashSet<Type> KnownEmptyTypes = new HashSet<Type>
                                                                    {
                                                                        typeof(ProcessCompletedCall),
                                                                        typeof(MarketDataSpecification),
                                                                        typeof(InterestRateCurveSensitivity),
                                                                        typeof(MultipleCurrencyInterestRateCurveSensitivity),
                                                                        typeof(SmileDeltaTermStructureParameter),
                                                                        typeof(DoubleLabelledMatrix3D),
                                                                        typeof(PdeGreekResultCollection),
                                                                        typeof(PdeFullResults1D),
                                                                        typeof(BucketedGreekResultCollection),
                                                                        typeof(ForwardCurve),
                                                                        typeof(BlackVolatilitySurfaceMoneyness),
                                                                        typeof(LocalVolatilitySurfaceMoneyness),
                                                                        typeof(ForwardCurveYieldImplied)
                                                                    };
        [Theory]
        [TypedPropertyData("MappedTypes")]
        public void TypesArentUseless(Type mappedType)
        {
            if (KnownEmptyTypes.Contains(mappedType))
            {
                return;
            }
            if (mappedType.GetCustomAttributes(false).Length > 0)
                return;
            if (mappedType.IsInterface)
                return;
            if (mappedType.GetNestedTypes().Any())
                return;
            const TypeAttributes staticTypeAttributes = TypeAttributes.Abstract | TypeAttributes.Sealed;
            if ((mappedType.Attributes & staticTypeAttributes) == staticTypeAttributes)
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
