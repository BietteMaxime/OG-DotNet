//-----------------------------------------------------------------------
// <copyright file="TypedPropertyDataAttribute.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Extensions;

namespace OGDotNet.Tests.Xunit.Extensions
{
    /// <summary>
    /// This is a slightly modified version of <see cref="PropertyDataAttribute"/> which allows better typing.
    /// It accepts <see cref="IEnumerable{T}"/> return types for one argument methods
    /// It also allows properties from the base class to be used
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class TypedPropertyDataAttribute : DataAttribute
    {
        private readonly string _propertyName;

        public TypedPropertyDataAttribute(string propertyName)
        {
            _propertyName = propertyName;
        }

        public override IEnumerable<object[]> GetData(MethodInfo methodUnderTest, Type[] parameterTypes)
        {
            Type declaringType = methodUnderTest.DeclaringType;
            PropertyInfo property = GetProperty(declaringType);
            object valueSource = property.GetValue(null, null);
            return TypeCooerceValues(methodUnderTest, declaringType, valueSource);
        }

        private PropertyInfo GetProperty(Type declaringType)
        {
            PropertyInfo property = declaringType.GetProperty(_propertyName, BindingFlags.Public | BindingFlags.Static);
            if (property == null)
            {
                if (declaringType.BaseType == null)
                {
                    throw new ArgumentException(string.Format("Could not find public static property {0} on {1}", _propertyName, declaringType.FullName));
                }
                else
                {
                    return GetProperty(declaringType.BaseType);
                }
            }
            return property;
        }

        private IEnumerable<object[]> TypeCooerceValues(MethodInfo methodUnderTest, Type declaringType, object valueSource)
        {
            if (valueSource == null)
            {
                return null;
            }
            if (valueSource is IEnumerable<object[]>)
            {
                return (IEnumerable<object[]>)valueSource;
            }

            var type = valueSource.GetType();
            var interfaces = type.GetInterfaces();
            var ienumInterface = interfaces.Where(i => i.FullName.StartsWith("System.Collections.Generic.IEnumerable`1")).ToList().FirstOrDefault();
            if (ienumInterface != null)
            {
                var enumType = ienumInterface.GetGenericArguments().First();
                switch (methodUnderTest.GetParameters().Length)
                {
                    case 1:
                        if (methodUnderTest.GetParameters()[0].ParameterType.IsAssignableFrom(enumType))
                        {
                            return ((IEnumerable)valueSource).Cast<object>().Select(o => new[] { o });
                        }
                        else
                        {
                            throw new ArgumentException(string.Format("I don't know how to pass {0} to parameter ({1})", enumType, methodUnderTest.GetParameters()[0]));
                        }
                    default:
                        throw new NotImplementedException("TODO: should probably handle tuples and/or property bags here");
                }
            }

            throw new ArgumentException(string.Format("Property {0} on {1} did not return IEnumerable<object[]> or an IEnumerable<T>", _propertyName, declaringType.FullName));
        }
    }
}
