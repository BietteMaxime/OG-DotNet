//-----------------------------------------------------------------------
// <copyright file="TestUtils.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace OGDotNet.Tests.Integration.Xunit.Extensions
{
    public static class TestUtils
    {
        public static string GetUniqueName()
        {
            return string.Format("{0}-{1}", ExecutingTestName, Guid.NewGuid());
        }

        public static  string ExecutingTestName
        {
            get
            {
                StackTrace stackTrace = new StackTrace();

                var testFrames = stackTrace.GetFrames().SkipWhile(f => f.GetMethod().DeclaringType.FullName.StartsWith(typeof(TestUtils).FullName)).TakeWhile(f => !f.GetMethod().DeclaringType.FullName.StartsWith("Xunit.Sdk")).ToList();
                var frames = testFrames.Last(f => f.GetMethod().DeclaringType.FullName.StartsWith("OGDotNet"));
                return string.Format("{0}.{1}", frames.GetMethod().DeclaringType.Name, frames.GetMethod().Name);
            }
        }
    }
}
