//-----------------------------------------------------------------------
// <copyright file="RestExceptionMapping.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using Fudge;
using OGDotNet.Mappedtypes;
using OGDotNet.Mappedtypes.engine.View.listener;
using OGDotNet.Mappedtypes.engine.View.permission;

namespace OGDotNet.Model
{
    public static class RestExceptionMapping
    {
        public static void DoWithExceptionMapping(Action action)
        {
            GetWithExceptionMapping(() => { action(); return 0; });
        }
        public static TRet GetWithExceptionMapping<TRet>(Func<TRet> func)
        {
            try
            {
                return func();
            }
            catch (WebException e)
            {
                JavaException exception;
                if (TryGetJavaException(e, out exception))
                {
                    throw exception.BuildException();
                }
                throw; // TODO should probably wrap in this in something generic and less Webby
            }
        }

        private static bool TryGetJavaException(WebException e, out JavaException exception)
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

                    if (messages == null)
                    {
                        exception = new JavaException(type);
                        return true;
                    }
                    if (messages.Length > 1)
                        throw new ArgumentException("Too many exception messages");

                    string message = messages[0];
                    exception = new JavaException(type, message);
                    return true;
                }
            }
            exception = null;
            return false;
        }
    }
}