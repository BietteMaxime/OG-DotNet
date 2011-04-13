//-----------------------------------------------------------------------
// <copyright file="ExceptionUtility.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Reflection;

namespace OGDotNet.Tests.Integration.Xunit.Extensions
{
    internal static class ExceptionUtility 
    {
        public static Exception GetExceptionWithStackTrace(Exception inner, StackTrace timedOutTrace)
        {
            //Tweaked from Xunit.Sdk.ExceptionUtility.RethrowWithNoStackTraceLoss
            (typeof(Exception).GetField("_remoteStackTraceString", BindingFlags.NonPublic | BindingFlags.Instance) ?? typeof(Exception).GetField("remote_stack_trace", BindingFlags.NonPublic | BindingFlags.Instance)).
                SetValue(inner, timedOutTrace.ToString() + "$$RethrowMarker$$");
            return inner;
        }

        public static void RethrowWithNoStackTraceLoss(Exception innerEx)
        {
            global::Xunit.Sdk.ExceptionUtility.RethrowWithNoStackTraceLoss(innerEx);
        }
    }
}