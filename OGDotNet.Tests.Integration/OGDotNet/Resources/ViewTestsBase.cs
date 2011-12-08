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
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Tests.Integration.Xunit.Extensions;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class ViewTestsBase : TestWithContextBase
    {
        const string EnvVarName = "GDotNet.Tests.Integration.DefinitionNames";

        private static readonly string InterestingView = null; // Useful for debugging

        protected static readonly HashSet<string> BannedViews = new HashSet<string>
                                                                  {
                                                                      // Broken
                                                                      "Cash Equity Detailed Test View",
                                                                      "Cash Equity Test View",
                                                                      "Multi-Currency Swap Test View (2)",
                                                                      "Equity Futures View TradelessPosition but Trade TargetType",
                                                                      "Simple CMS Cap/Floor Test View",
                                                                      "Multi-currency Equity Option Test View",
                                                                      "Simple Ibor Cap/Floor Test View",
                                                                      "Simple IR Future Option Test View (2)",
                                                                      "Simple IR Future Option Test View" //PLAT-1459
                                                                  };

        protected static readonly HashSet<string> FastTickingViews = new HashSet<string>
                                                                  {
                                                                      "Equity Option Test View 1",
                                                                      "Demo Equity Option Test View",
                                                                      "Simple Swap Test View",
                                                                      "Simple Swaption Test View",
                                                                      "Simple FRA Test View"
                                                                  };

        public static IEnumerable<ViewDefinition> ViewDefinitions
        {
            get
            {
                HashSet<string> selectedNames = null;
                var envViews = Environment.GetEnvironmentVariable(EnvVarName);
                if (envViews != null)
                {
                    selectedNames = new HashSet<string>(envViews.Split(';'));
                }
                if (InterestingView != null)
                {
                    selectedNames = new HashSet<string> { InterestingView };
                }

                var remoteEngineContext = Context;
                var definitionRepository = remoteEngineContext.ViewProcessor.ViewDefinitionRepository;
                var included = definitionRepository
                    .GetDefinitionEntries()
                    .Where(e => selectedNames == null ? IsNotBanned(e.Value) : selectedNames.Contains(e.Value));
                var definitions = included
                    .Select(o => definitionRepository.GetViewDefinition(o.Key))
                    .ToList();
                return definitions;
            }
        }

        private static bool IsNotBanned(string n)
        {
            return !BannedViews.Contains(n) 
                && !TestUtils.ContainsGuid(n) 
                && !n.Contains("web form test") 
                && !n.Contains("web test")
                && !n.EndsWith("(afshin)")
                && !n.Contains("Sandbox")
                && n.Any(c => char.IsLower(c));
        }

        public static IEnumerable<ViewDefinition> FastTickingViewDefinitions
        {
            get
            {
                return ViewDefinitions.Where(n => FastTickingViews.Contains(n.Name));
            }
        }
    }
}