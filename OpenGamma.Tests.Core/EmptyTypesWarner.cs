// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EmptyTypesWarner.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using OpenGamma.Analytics.Financial.InterestRate;
using OpenGamma.Analytics.Financial.Model.FiniteDifference;
using OpenGamma.Analytics.Financial.Model.InterestRate.Curve;
using OpenGamma.Analytics.Financial.Model.Option.Definition;
using OpenGamma.Analytics.Financial.Model.Volatility.Surface;
using OpenGamma.Engine.MarketData.Spec;
using OpenGamma.Engine.View.Listener;
using OpenGamma.Financial.Analytics;
using OpenGamma.Financial.Forex.Method;
using OpenGamma.Financial.Greeks;
using OpenGamma.Id;
using OpenGamma.Xunit.Extensions;

using Xunit;
using Xunit.Extensions;

namespace OpenGamma
{
    public class EmptyTypesWarner
    {
        public static IEnumerable<Type> MappedTypes
        {
            get
            {
                var mappedTypes = typeof(UniqueId).Assembly.GetTypes().Where(t => t.FullName.StartsWith("OpenGamma")).ToList();
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
