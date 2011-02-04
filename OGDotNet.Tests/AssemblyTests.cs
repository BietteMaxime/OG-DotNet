using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using OGDotNet.AnalyticsViewer.View;
using OGDotNet.SecurityViewer.View;
using OGDotNet.Utils;
using OGDotNet.WPFUtils;
using Xunit;
using Xunit.Extensions;

namespace OGDotNet.Tests
{
    public class AssemblyTests
    {
        private static readonly IEnumerable<Assembly> Assemblies =
            new [] {typeof (DisposableBase), typeof (MainWindow), typeof (SecurityWindow), typeof (BindingUtils)}.
                Select(t => t.Assembly);
        [Fact]
        public void CopyrightIsSane()
        {
            const string companyName = "OpenGamma Limited";

            foreach (var assembly in Assemblies)
            {
                var company = GetLoneAttribute<AssemblyCompanyAttribute>(assembly);
                Assert.Equal(companyName, company.Company);
                var copyright = GetLoneAttribute<AssemblyCopyrightAttribute>(assembly);

                Assert.Equal(string.Format("Copyright © {0} {1}", companyName, DateTime.Now.Year), copyright.Copyright);
            }
        }

        private static T GetLoneAttribute<T>(Assembly assembly) where T : Attribute
        {
            return (T) assembly.GetCustomAttributes(typeof (T), false).First();
        }
    }
}
