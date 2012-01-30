//-----------------------------------------------------------------------
// <copyright file="FinancialClientTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using FactAttribute = OGDotNet.Tests.Integration.Xunit.Extensions.FactAttribute;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class FinancialClientTests : TestWithContextBase
    {
        [Fact]
        public void CanCreateAndDispose()
        {
            using (Context.CreateFinancialClient())
            {
            }
        }
        
        //TODO heartbeat test

        [Fact]
        public void CanGetAllServices()
        {
            using (var client = Context.CreateFinancialClient())
            {
                var type = client.GetType();
                foreach (var prop in type.GetProperties())
                {
                    var service = prop.GetGetMethod().Invoke(client, new object[] { });
                    if (service == null)
                    {
                        throw new Exception("Null service " + prop.Name);
                    }
                }
                foreach (var method in type.GetMethods().Where(m => !m.IsSpecialName && m.GetParameters().Count() == 0 && m.DeclaringType != typeof(object)))
                {
                    if (method.Name == "Dispose")
                    {
                        continue;
                    }
                    var result = method.Invoke(client, new object[] { });
                    if (result == null)
                    {
                        throw new Exception("Null service " + method.Name);
                    }
                    if (result is IDisposable)
                    {
                        ((IDisposable)result).Dispose();
                    }
                }
            }
        }
    }
}
