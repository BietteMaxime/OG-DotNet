// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypedPropertyDataAttribute.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using OpenGamma.Util;

using Xunit.Extensions;

namespace OpenGamma.Xunit.Extensions
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
                var enumerableSource = ((IEnumerable)valueSource).Cast<object>();
                var enumType = ienumInterface.GetGenericArguments().First();
                var paramsCount = methodUnderTest.GetParameters().Length;
                switch (paramsCount)
                {
                    case 1:
                        if (methodUnderTest.GetParameters()[0].ParameterType.IsAssignableFrom(enumType))
                        {
                            return enumerableSource.Select(o => new[] { o });
                        }
                        else
                        {
                            throw new ArgumentException(string.Format("I don't know how to pass {0} to parameter ({1})", enumType, methodUnderTest.GetParameters()[0]));
                        }

                    case 3:
                        if (!enumType.IsGenericType || enumType.GetGenericTypeDefinition() != typeof(Tuple<,,>))
                        {
                            throw new ArgumentException(string.Format("I don't know how to pass {0} to 3 parameters, use Tuple", enumType));
                        }

                        for (int i = 0; i < paramsCount; i++)
                        {
                            CheckTypeParameter(0, methodUnderTest, enumType);
                        }

                        var args = enumerableSource.Select(t => Enumerable.Range(0, 3).Select(i => GetTupleItem(t, i)).ToArray()).ToList();
                        return args;
                    default:
                        throw new NotImplementedException("TODO: should probably handle tuples and/or property bags here");
                }
            }

            throw new ArgumentException(string.Format("Property {0} on {1} did not return IEnumerable<object[]> or an IEnumerable<T>", _propertyName, declaringType.FullName));
        }

        private static object GetTupleItem(object tuple, int i)
        {
            var type = tuple.GetType();
            MethodInfo getMethod = GetGetMethod(type, i);
            return getMethod.Invoke(tuple, new object[] {});
        }

        static readonly Memoizer<Type, int, MethodInfo> GetGetMethodCache = new Memoizer<Type, int, MethodInfo>(GetGetMethodImpl);
        private static MethodInfo GetGetMethod(Type type, int i)
        {
            return GetGetMethodCache.Get(type, i);
        }

        private static MethodInfo GetGetMethodImpl(Type type, int i)
        {
            var propertyInfo = type.GetProperty("Item" + (i + 1));
            if (propertyInfo == null)
            {
                throw new ArgumentException(string.Format("Don't know how to get item {0} from {1}", i, type));
            }

            return propertyInfo.GetGetMethod();
        }

        private static void CheckTypeParameter(int index, MethodInfo methodUnderTest, Type enumType)
        {
            var paramtype = methodUnderTest.GetParameters()[index].ParameterType;
            var tupleType = enumType.GetGenericArguments()[index];
            if (tupleType != paramtype && paramtype.IsAssignableFrom(tupleType))
            {
                throw new ArgumentException(string.Format("I don't know how to pass {0} to parameter ({1})", tupleType, paramtype));
            }
        }
    }
}
