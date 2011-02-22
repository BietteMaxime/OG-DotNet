using System;
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
                                                                      "jonathan/b1232530-38ed-11e0-8000-541213631ee5/Test Bond View (0)",
                                                                      "Bond Future Test View",
                                                                      "Cash Equity Detailed Test View",
                                                                      "Cash Equity Test View",
                                                                      "Swap Test View",
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
                var remoteEngineContext = GetContext();
                return remoteEngineContext.ViewProcessor.ViewNames.Where(IsNotBanned);
            }
        }

        public static IEnumerable<RemoteView> Views
        {
            get
            {
                var remoteEngineContext = GetContext();
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
                var remoteEngineContext = GetContext();
                return ViewNames.Where(n => !BannedViews.Contains(n)).Select(n => remoteEngineContext.ViewProcessor.GetView(n)).Where(n => ! IsSlowTickingView(n));
            }
        }
    }
}