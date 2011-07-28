//-----------------------------------------------------------------------
// <copyright file="ViewTestsBaseTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using Xunit;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class ViewTestsBaseTests : ViewTestsBase
    {
        [Xunit.Extensions.Fact]
        public void AllBannedViewsExist()
        {
            var definitionNames = Context.ViewProcessor.ViewDefinitionRepository.GetDefinitionNames();
            foreach (var bannedView in BannedViews)
            {
                Assert.Contains(bannedView, definitionNames);
            }
        }
    }
}