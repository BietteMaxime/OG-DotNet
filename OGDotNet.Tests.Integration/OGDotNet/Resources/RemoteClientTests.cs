//-----------------------------------------------------------------------
// <copyright file="RemoteClientTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Tests.Integration.OGDotNet.Model.Context;
using FactAttribute = OGDotNet.Tests.Integration.Xunit.Extensions.FactAttribute;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteClientTests
    {
        [Fact]
        public void CanCreateAndDispose()
        {
            using (RemoteEngineContextTests.GetContext().CreateUserClient())
            {
            }
        }
        
        //TODO heartbeat test
    }
}
