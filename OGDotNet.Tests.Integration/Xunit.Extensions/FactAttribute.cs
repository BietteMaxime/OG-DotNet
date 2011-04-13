//-----------------------------------------------------------------------
// <copyright file="FactAttribute.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Xunit.Sdk;

namespace OGDotNet.Tests.Integration.Xunit.Extensions
{
    public class FactAttribute : global::Xunit.FactAttribute
    {
        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            var enumerateTestCommands = base.EnumerateTestCommands(method);

            return enumerateTestCommands.Select(cmd => new CustomizingCommand(cmd));
        }
    }
}