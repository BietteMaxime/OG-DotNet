//-----------------------------------------------------------------------
// <copyright file="ViewTestsBase.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OGDotNet.Mappedtypes.engine.view;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class ViewTestsBase : TestWithContextBase
    {
        private static readonly string InterestingView = null; // Useful for debugging

        private static readonly HashSet<string> BannedViews = new HashSet<string>
                                                                  {
                                                                      // Slow
                                                                      "10K Swap Test View",

                                                                      // Broken
                                                                      "TestDefinition",
                                                                      "Primitives Only",
                                                                      "OvernightBatchTestView",
                                                                      "GlobeOp Bond View",
                                                                      "European Corporate Bond View"
                                                                  };

        private static bool IsSlowTicking(string definitionName)
        {
            if (definitionName == InterestingView)
                return false;
            if (definitionName == "Primitives Only")
                return true;
            if (definitionName == "Bond Future Test View")
                return true;
            if (definitionName.StartsWith("Cash Equity"))
                return true;
            if (definitionName.StartsWith("GlobeOp Bond View"))
                return true;
            return false;
        }

        public static IEnumerable<string> DefinitionNames
        {
            get
            {
                var remoteEngineContext = Context;
                var definitionNames = remoteEngineContext.ViewProcessor.ViewDefinitionRepository.GetDefinitionNames();
                var ret = InterestingView == null ? definitionNames.Where(IsNotBanned) : Enumerable.Repeat(InterestingView, 1);
                if (ret.Any(e => e == null))
                {
                    throw new ArgumentException();
                }
                return ret;
            }
        }

        public static IEnumerable<ViewDefinition> ViewDefinitions
        {
            get
            {
                var remoteEngineContext = Context;
                return DefinitionNames.Select(n => remoteEngineContext.ViewProcessor.ViewDefinitionRepository.GetViewDefinition(n));
            }
        }

        private static bool IsNotBanned(string n)
        {
            return !BannedViews.Contains(n) && !ContainsGuid(n);
        }

        private static readonly Regex GuidRegex = new Regex(@"\w{8}\-\w{4}\-\w{4}\-\w{4}\-\w{12}", RegexOptions.Compiled);
        private static bool ContainsGuid(string s)
        {
            var containsGuid = GuidRegex.IsMatch(s);
            return containsGuid;
        }

        public static IEnumerable<ViewDefinition> FastTickingViewDefinitions
        {
            get
            {
                return ViewDefinitions.Where(n => !IsSlowTicking(n.Name));
            }
        }
    }
}