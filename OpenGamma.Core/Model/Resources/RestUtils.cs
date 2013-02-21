// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestUtils.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Reflection;

using OpenGamma.Id;

namespace OpenGamma.Model.Resources
{
    public static class RestUtils
    {
        public static RestTarget EncodeQueryParams(RestTarget baseTarget, object bean)
        {
            // TODO [DOTNET-14] could use fudge encoder proper if we encoded IDs properly 
            var ret = baseTarget;
            foreach (var property in bean.GetType().GetProperties())
            {
                ret = ret.WithParam(EncodePropName(property), EncodeParamValue(GetPropertyValue(bean, property)));    
            }
            
            return ret;
        }

        private static string EncodePropName(PropertyInfo property)
        {
            var name = property.Name;
            return name.Substring(0, 1).ToLower() + name.Substring(1);
        }

        private static object GetPropertyValue(object bean, PropertyInfo property)
        {
            return property.GetGetMethod().Invoke(bean, null);
        }

        private static string EncodeParamValue(object value)
        {
            if (value is string)
            {
                return (string) value;
            }
            else if (value is int || value is ObjectId || value is bool)
            {
                return value.ToString();
            }
            else
            {
                throw new ArgumentException("Don't know how to rest encode " + value);
            }
        }
    }
}