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
using OGDotNet.Model.Resources;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class ViewTestsBase : TestWithContextBase
    {
        private static readonly HashSet<string> BannedViews = new HashSet<string>
                                                                  {
                                                                      // Slow
                                                                      "10K Swap Test View",

                                                                      // Broken
                                                                      "TestDefinition",
                                                                      "Swap Test View",
                                                                      "Primitives Only",
                                                                      "OvernightBatchTestView",
                                                                      "Equity Strategies View 1",
                                                                      "Multi-currency Equity Option Test View",
                                                                      "GlobeOp Bond View",
                                                                      "European Corporate Bond View"
                                                                  };

        private static bool IsSlowTicking(string definitionName)
        {
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
                return remoteEngineContext.ViewProcessor.ViewDefinitionRepository.GetDefinitionNames().Where(IsNotBanned);
            }
        }

        public static IEnumerable<ViewDefinition> ViewDefinitions
        {
            get
            {
                var remoteEngineContext = Context;
                return DefinitionNames.Where(IsNotBanned).Select(n => remoteEngineContext.ViewProcessor.ViewDefinitionRepository.GetViewDefinition(n));
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