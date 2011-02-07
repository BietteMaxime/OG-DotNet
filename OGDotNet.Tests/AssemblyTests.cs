using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OGDotNet.AnalyticsViewer.View;
using OGDotNet.SecurityViewer.View;
using Xunit;

namespace OGDotNet.Tests
{
    public class AssemblyTests
    {
        private static readonly  Type[] ForcedReferences = new[]{typeof(MainWindow), typeof(SecurityWindow)};
        private static readonly string Namespace = typeof (AssemblyTests).Namespace.Replace(".Tests","");
        private static readonly IEnumerable<Assembly> Assemblies =GetAssemblies();

        [Fact]
        public void CopyrightIsSane()
        {
            const string companyName = "OpenGamma Limited";

            var expected = string.Format("Copyright © {0} {1}", companyName, DateTime.Now.Year);

            foreach (var assembly in Assemblies)
            {
                var company = GetLoneAttribute<AssemblyCompanyAttribute>(assembly);
                Assert.Equal(companyName, company.Company);
                var copyright = GetLoneAttribute<AssemblyCopyrightAttribute>(assembly);

                Assert.Equal(expected, copyright.Copyright);
            }
        }

        [Fact]
        public void VersionsMatch()
        {
            Assert.Equal(1, Assemblies.Select(a => a.GetName().Version).Distinct().Count());
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            foreach (var variable in ForcedReferences)
            {
                //These are here to avoid the compiler trimming the references
            }

            return GetAllDependencies(n => n.Name.StartsWith(Namespace), Assembly.GetExecutingAssembly().GetName()).Distinct().ToList();
            
        }

        private static IEnumerable<Assembly> GetAllDependencies(Predicate<AssemblyName> filter, AssemblyName root)
        {
            if (!filter(root))
                yield break;
            var rootAssembly = Assembly.Load(root);
            yield return rootAssembly;
            foreach (var referencedAssembly in rootAssembly.GetReferencedAssemblies())
            {
                foreach (var recurse in GetAllDependencies(filter, referencedAssembly))
                {
                    yield return recurse;
                }
            }
        }

        private static T GetLoneAttribute<T>(Assembly assembly) where T : Attribute
        {
            return (T) assembly.GetCustomAttributes(typeof (T), false).First();
        }
    }
}
