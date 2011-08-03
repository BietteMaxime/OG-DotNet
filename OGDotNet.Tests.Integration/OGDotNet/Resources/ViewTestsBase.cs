﻿//-----------------------------------------------------------------------
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
        private static readonly string InterestingView = null; // Useful for debugging

        protected static readonly HashSet<string> BannedViews = new HashSet<string>
                                                                  {
                                                                      // Slow
                                                                      "10K USD Swap Test View",

                                                                      // Broken
                                                                      "OvernightBatchTestView",
                                                                      "GlobeOp Bond View",
                                                                      "GlobeOp Bond View Implied",
                                                                      "Cash Equity Detailed Test View",
                                                                      "Cash Equity Test View",
                                                                      "European Corporate Bond View",
                                                                      "European Corporate Bond View - test",
                                                                      "Random Matrix",
                                                                      "Funding Curve Only",
                                                                      "Simple IR Future Option Test View" //PLAT-1459
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
            return !BannedViews.Contains(n) && !TestUtils.ContainsGuid(n) && !n.Contains("web form test") && !n.Contains("web test");
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