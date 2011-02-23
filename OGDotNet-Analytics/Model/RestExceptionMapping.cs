using System;
using System.Net;
using System.Reflection;

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

        private static Exception BuildException(string javaType)
        {
            Type exceptionType = GetExceptionType(javaType);

            ConstructorInfo constructorInfo = exceptionType.GetConstructor(new Type[] {});
            if (constructorInfo == null)
                throw new ArgumentException(String.Format("Can't construct exception type {0}->{1}", javaType, exceptionType), "javaType");
            return (Exception)constructorInfo.Invoke(new object[] {});
        }

        private static Exception BuildException(string javaType, string message)
        {
            Type exceptionType = GetExceptionType(javaType);

            ConstructorInfo constructorInfo = exceptionType.GetConstructor(new[] {typeof(string)});
            if (constructorInfo == null)
                throw new ArgumentException(String.Format("Can't construct exception type {0}->{1}",javaType,exceptionType),"javaType");
            if (constructorInfo.GetParameters()[0].Name != "message")
                throw new ArgumentException(String.Format("Exception type {0}->{1} expectes {2} not message" , javaType, exceptionType, message), "javaType");
            return (Exception) constructorInfo.Invoke(new object[] {message});
        }

        private static Type GetExceptionType(string javaType)
        {
            switch (javaType)
            {
                case "java.lang.IllegalArgumentException":
                    return typeof (ArgumentException);
                case "java.lang.NullPointerException":
                    return typeof (NullReferenceException);
                //TODO case "com.opengamma.OpenGammaRuntimeException":
                //TODO case "org.fudgemsg.FudgeRuntimeException":
                //TODO case "java.lang.IllegalStateException":
                default:
                    return typeof(Exception);
            }
        }
    }
}