//-----------------------------------------------------------------------
// <copyright file="UriHacks.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using System.Reflection;
using OGDotNet.Utils;

namespace OGDotNet.Model
{
    internal static class UriHacks
    {
        private static readonly int UnEscapeDotsAndSlashes;
        private static readonly FieldInfo SyntaxField;
        private static readonly FieldInfo FlagsField;

        static UriHacks()
        {
            var flagsType = typeof(Uri).Assembly.GetType("System.UriSyntaxFlags");
            UnEscapeDotsAndSlashes = (int)flagsType.GetFields().First(f => f.Name == "UnEscapeDotsAndSlashes").GetValue(null);

            SyntaxField = typeof(Uri).GetField("m_Syntax", BindingFlags.Instance | BindingFlags.NonPublic);
            if (SyntaxField.FieldType != typeof(UriParser))
                throw new Exception("This horrible hack has bitten me, System.Uri is not as expected here (  http://connect.microsoft.com/VisualStudio/feedback/details/94109/system-uri-constructor-evaluates-escaped-slashes-and-removes-double-slashes )");

            FlagsField = typeof(UriParser).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);
            if (FlagsField.FieldType != flagsType)
                throw new Exception("This horrible hack has bitten me, System.Uri is not as expected here (  http://connect.microsoft.com/VisualStudio/feedback/details/94109/system-uri-constructor-evaluates-escaped-slashes-and-removes-double-slashes )");
        }

        public static void LeaveDotsAndSlashesEscaped(Uri uri)
        {
            //This is grim http://connect.microsoft.com/VisualStudio/feedback/details/94109/system-uri-constructor-evaluates-escaped-slashes-and-removes-double-slashes
            ArgumentChecker.NotNull(uri, "uri");

            object uriParser = SyntaxField.GetValue(uri);

            object uriSyntaxFlags = FlagsField.GetValue(uriParser);

            // Clear the flag that we don't want
            uriSyntaxFlags = (int)uriSyntaxFlags & ~UnEscapeDotsAndSlashes;

            FlagsField.SetValue(uriParser, uriSyntaxFlags);
        }
    }
}