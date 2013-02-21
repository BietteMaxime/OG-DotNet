// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenericUtils.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Reflection;

namespace OpenGamma.Util
{
    public static class GenericUtils
    {
        static readonly Memoizer<Type, string, MethodInfo> GenericMethod = new Memoizer<Type, string, MethodInfo>(GetGenericMethodImpl);
        static readonly Memoizer<Type, Type, Type[]> GenericArgs = new Memoizer<Type, Type, Type[]>(GetGenericArgs);
        static readonly Memoizer<MethodInfo, Type[], MethodInfo> GenericMethodMaker = new Memoizer<MethodInfo, Type[], MethodInfo>(MakeGenericMethod);

        public static MethodInfo GetGenericMethod(Type genericMethodContainingType, string genericMethodName)
        {
            return GenericMethod.Get(genericMethodContainingType, genericMethodName);
        }

        private static MethodInfo GetGenericMethodImpl(Type genericMethodContainingType, string genericMethodName)
        {
            return genericMethodContainingType.GetMethods().Where(m => m.Name == genericMethodName && m.IsGenericMethodDefinition).Single();
        }

        public static T Call<T>(Type genericMethodContainingType, string genericMethodName, Type genericTypeDefinition, params object[] args)
        {
            return (T) Call(genericMethodContainingType, genericMethodName, genericTypeDefinition, args);
        }

        public static object Call(Type genericMethodContainingType, string genericMethodName, Type genericTypeDefinition, params object[] args)
        {
            var genericMethodDefinition = GetGenericMethod(genericMethodContainingType, genericMethodName);
            return Call(genericMethodDefinition, genericTypeDefinition, args);
        }

        private static Type[] GetGenericArgs(Type genericTypeDefinition, Type genericType)
        {
            ArgumentChecker.NotNull(genericType, "genericType");
            if (genericType == typeof(object))
            {
                throw new ArgumentException(string.Format("{0} doesn't provide {1}", genericType, genericTypeDefinition), "genericType");
            }

            if (genericType.IsGenericType && genericType.GetGenericTypeDefinition() == genericTypeDefinition)
            {
                return genericType.GetGenericArguments();
            }

            foreach (var i in genericType.GetInterfaces().Concat(new[] { genericType.BaseType }))
            {
                if ( i == genericType)
                {
                    continue;
                }

                try
                {
                    Type[] args = GetGenericArgs(genericTypeDefinition, i);
                    return args;
                }
                catch (ArgumentException)
                {
                }
            }

            throw new ArgumentException(string.Format("{0} doesn't provide {1}", genericType, genericTypeDefinition), "genericType");
        }

        public static object Call(MethodInfo genericMethodDefinition, Type genericTypeDefinition, params object[] args)
        {
            ArgumentChecker.Not(!genericTypeDefinition.IsGenericTypeDefinition, "genericTypeDefinition");
            ArgumentChecker.NotEmpty(args, "args");
            var genericType = args[0].GetType();
            var genericArguments = GenericArgs.Get(genericTypeDefinition, genericType);
            
            return Call(genericMethodDefinition, genericArguments, args);
        }

        private static MethodInfo MakeGenericMethod(MethodInfo genericMethodDefinition, Type[] genericArguments)
        {
            ArgumentChecker.Not(!genericMethodDefinition.IsStatic, "genericMethodDefinition");
            ArgumentChecker.Not(!genericMethodDefinition.IsGenericMethodDefinition, "genericMethodDefinition");

            return genericMethodDefinition.MakeGenericMethod(genericArguments);
        }

        public static object Call(MethodInfo genericMethodDefinition, Type[] genericArguments, params object[] args)
        {
            // TODO could use emit for more speed
            var genericMethod = GenericMethodMaker.Get(genericMethodDefinition, genericArguments);
            return genericMethod.Invoke(null, args);
        }
    }
}
