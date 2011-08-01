//-----------------------------------------------------------------------
// <copyright file="TestNGRunner.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OGDotNet.Tests.ExecBuilderTestProxy
{
    public static class TestNGRunner
    {
        static readonly string OGPlatformLocation;
        static readonly string JavaHome;

        static TestNGRunner()
        {
            OGPlatformLocation = Environment.GetEnvironmentVariable("OG_PLATFORM_PATH");
            JavaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
        }

        private static string Lines(IEnumerable<string> lines)
        {
            return string.Join(Environment.NewLine, lines);
        }
        public static void CallTestNG()
        {
            const string docTemplate =
                "<!DOCTYPE suite SYSTEM \"http://testng.org/testng-1.0.dtd\"><suite name=\"OGDotNetBuilderTests\">{0}</suite>";
            const string testTemplate = "<test name=\"{0}\"><classes>{1}</classes></test>";
            const string classTemplate = "<class name=\"{0}\" />";

            var testsXml = Lines(
                        BuilderTestClassesByProject.Select( kvp => string.Format(testTemplate, kvp.Key, Lines(kvp.Value.Select(c => string.Format(classTemplate, c))))));
            string testNgXml = String.Format(docTemplate, testsXml);
            const string xmlFileName = "testng.xml";
            File.WriteAllText(xmlFileName, testNgXml);

            try
            {
                CallTestNG(xmlFileName, BuilderTestClassesByProject.Keys.ToArray());
            }
            finally
            {
                File.Delete(xmlFileName);
            }
        }

        private static void CallTestNG(string xmlFileName, params string[] projects)
        {
            string java = Path.Combine(JavaHome, "bin", "java.exe");
            if (! File.Exists(java))
            {
                throw new Exception(@"Couldn't find java");
            }
            string classPath = GetClassPath(projects);

            const string propertyName = @"com.opengamma.util.test.BuilderTestProxyFactory.ExecBuilderTestProxy.execPath";
            string propertyvalue = Path.GetFileName(Assembly.GetExecutingAssembly().Location);

            string arguments = String.Format(@"-classpath {0} -D{1}={2} org.testng.TestNG {3}", classPath, propertyName, propertyvalue, xmlFileName);
            var processStartInfo = new ProcessStartInfo(java, arguments)
                                       {
                                           UseShellExecute = false
                                       };
            processStartInfo.EnvironmentVariables.Add("com.opengamma.util.test.BuilderTestProxyFactory.ExecBuilderTestProxy.execPath", Assembly.GetExecutingAssembly().Location);

            using (var process = new Process())
            {
                Console.Error.WriteLine(@"Starting process {0} {1}", processStartInfo.FileName, processStartInfo.Arguments);
                process.StartInfo = processStartInfo;
                process.Start();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    Console.Error.WriteLine(@"Tests not run succesfully");
                }
            }
        }

        private static string GetClassPath(params string[] projects)
        {
            return string.Join(";", projects.Select(GetClassPath));
        }

        private static string GetClassPath(string project)
        {
            string projectPath = string.Format(@"{0}\projects\{1}\", OGPlatformLocation, project);
            return string.Format(@"{0}\build\classes;{0}\lib\classpath.jar;{0}\tests\classes", projectPath);
        }

        private static Dictionary<string, string[]> BuilderTestClassesByProject
        {
            get
            {
                return new Dictionary<string, string[]>
                           {
                               {"OG-Engine", new[] {@"com.opengamma.engine.fudgemsg.ValuePropertiesBuilderTest"}},
                           };
            }
        }
    }
}