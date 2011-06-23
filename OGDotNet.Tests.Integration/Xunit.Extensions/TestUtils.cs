//-----------------------------------------------------------------------
// <copyright file="TestUtils.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace OGDotNet.Tests.Integration.Xunit.Extensions
{
    public static class TestUtils
    {
        public static string GetUniqueName()
        {
            return string.Format("{0}-{1}", ExecutingTestName, Guid.NewGuid());
        }

        public static string ExecutingTestName
        {
            get
            {
                StackTrace stackTrace = new StackTrace();

                var testFrames = stackTrace.GetFrames().SkipWhile(f => f.GetMethod().DeclaringType.FullName.StartsWith(typeof(TestUtils).FullName)).TakeWhile(f => !f.GetMethod().DeclaringType.FullName.StartsWith("Xunit.Sdk")).ToList();
                var frames = testFrames.Last(f => f.GetMethod().DeclaringType.FullName.StartsWith("OGDotNet"));
                return string.Format("{0}.{1}", frames.GetMethod().DeclaringType.Name, frames.GetMethod().Name);
            }
        }

        private static readonly Regex GuidRegex = new Regex(@"\w{8}\-\w{4}\-\w{4}\-\w{4}\-\w{12}", RegexOptions.Compiled);

        public static bool ContainsGuid(string s)
        {
            var containsGuid = GuidRegex.IsMatch(s);
            return containsGuid;
        }
    }
}
