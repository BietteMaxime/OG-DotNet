using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using OGDotNet.Mappedtypes;
using OGDotNet.Mappedtypes.engine.View.permission;

namespace OGDotNet.Model
{
    public static class RestExceptionMapping
    {
        public static void DoWithExceptionMapping(Action action)
        {
            GetWithExceptionMapping(() => { action();return 0;});
        }
        public static TRet GetWithExceptionMapping<TRet>(Func<TRet> func)
        {
            try
            {
                return func();
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    string[] types = e.Response.Headers.GetValues("X-OpenGamma-ExceptionType");
                    if (types != null)
                    {
                        if (types.Length > 1)
                            throw new ArgumentException("Too many exception types");
                        string type = types[0];


                        string[] messages = e.Response.Headers.GetValues("X-OpenGamma-ExceptionMessage");

                        if (messages==null)
                            throw BuildException(type);
                        if (messages.Length > 1)
                            throw new ArgumentException("Too many exception messages");
                        
                        string message = messages[0];
                        throw BuildException(type, message);
                    }
                }
                throw;//TODO should probably wrap in this in something generic and less Webby
            }
        }

        private static readonly IDictionary<string, Type> DotNetTypesByJavaTypeName = new Dictionary<string, Type>
                                                {
                    {"java.lang.IllegalArgumentException", typeof(ArgumentException)},
                    {"java.lang.NullPointerException", typeof(NullReferenceException)},
                    {"java.lang.IllegalStateException", typeof(InvalidOperationException)},
                    {"com.opengamma.engine.view.permission.ViewPermissionException", typeof(ViewPermissionException)},
                    {"com.opengamma.OpenGammaRuntimeException", typeof(OpenGammaException)},
                    //TODO "org.fudgemsg.FudgeRuntimeException":
                                                };

        private static Exception BuildException(string javaType, string message = null)
        {
            Type exceptionType;
            if (DotNetTypesByJavaTypeName.TryGetValue(javaType, out exceptionType))
            {
                if (message == null)
                {
                    ConstructorInfo constructorInfo = exceptionType.GetConstructor(new Type[] {});
                    if (constructorInfo == null)
                        throw new ArgumentException(String.Format("Can't construct exception type {0}->{1}", javaType, exceptionType),"javaType");

                    return (Exception) constructorInfo.Invoke(new object[] {});
                }

                else
                {
                    ConstructorInfo constructorInfo = exceptionType.GetConstructor(new[] {typeof (string)});

                    if (constructorInfo == null)
                        throw new ArgumentException(String.Format("Can't construct exception type {0}->{1}", javaType, exceptionType),"javaType");
                    if (constructorInfo.GetParameters()[0].Name != "message")
                        throw new ArgumentException(String.Format("Exception type {0}->{1} expectes {2} not message", javaType, exceptionType, message), "javaType");
                    return (Exception) constructorInfo.Invoke(new object[] {message});
                }
            }
            else
            {
                return BuildGenericException(javaType, message);
            }
        }


        private static Exception BuildGenericException(string javaType, string message = null)
        {
            return new Exception(string.Format("{0}: {1} - {2}", javaType, message, "Unmappable java exception ocurred"));
        }
    }
}