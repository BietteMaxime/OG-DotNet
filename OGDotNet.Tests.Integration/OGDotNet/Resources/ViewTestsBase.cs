//-----------------------------------------------------------------------
// <copyright file="ViewTestsBase.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OGDotNet.Model.Resources;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class ViewTestsBase : TestWithContextBase
    {
        private static readonly HashSet<string> BannedViews = new HashSet<string>
                                                                  {
                                                                      "10K Swap Test View",//Slow

                                                                      //Broken
                                                                      "TestDefinition",
                                                                      "Swap Test View",
                                                                      "Primitives Only",
                                                                      "OvernightBatchTestView",
                                                                      "Equity Strategies View 1"
                                                                  };

        private static bool IsSlowTickingView(RemoteView view)
        {
            if (view.Name == "Primitives Only")
                return true;
            if (view.Name == "Bond Future Test View")
                return true;
            if (view.Name.StartsWith("Cash Equity"))
                return true;
            return false;
        }

        public static IEnumerable<string> ViewNames
        {
            get
            {
                var remoteEngineContext = Context;
                return remoteEngineContext.ViewProcessor.ViewNames.Where(IsNotBanned);
            }
        }

        public static IEnumerable<RemoteView> Views
        {
            get
            {
                var remoteEngineContext = Context;
                return ViewNames.Where(IsNotBanned).Select(n => remoteEngineContext.ViewProcessor.GetView(n));
            }
        }

        private static bool IsNotBanned(string n)
        {
            return !BannedViews.Contains(n) && ! ContainsGuid(n);
        }

        private static readonly Regex GuidRegex = new Regex(@"\w{8}\-\w{4}\-\w{4}\-\w{4}\-\w{12}", RegexOptions.Compiled);
        private static bool ContainsGuid(string s)
        {
            var containsGuid = GuidRegex.IsMatch(s);
            return containsGuid;
        }

        public static IEnumerable<RemoteView> FastTickingViews
        {
            get
            {
                return Views.Where(n => ! IsSlowTickingView(n));
            }
        }
    }
}