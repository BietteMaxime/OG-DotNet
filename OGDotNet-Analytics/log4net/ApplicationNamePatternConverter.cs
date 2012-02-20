//-----------------------------------------------------------------------
// <copyright file="ApplicationNamePatternConverter.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.IO;
using System.Reflection;

namespace OGDotNet.Log4net
{
    public class ApplicationNamePatternConverter : log4net.Util.PatternConverter
    {
        protected override void Convert(TextWriter writer, object state)
        {
            var uaAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

            writer.Write(uaAssembly.GetName().Name);
        }
    }
}