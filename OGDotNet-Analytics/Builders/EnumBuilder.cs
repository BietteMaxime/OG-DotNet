//-----------------------------------------------------------------------
// <copyright file="EnumBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Utils;

namespace OGDotNet.Builders
{
    public class EnumBuilder<T> : 
        //NOTE: we don't implement BuilderBase<T> because then boxing would bite us
        BuilderBase<object> where T : struct
    {
        #region static utils
        private static readonly Memoizer<string, T> ParseTable = new Memoizer<string, T>(ParseImpl);
        private static readonly Memoizer<T, string> JavaNamesTable = new Memoizer<T, string>(GetJavaNameImpl);

        public static T Parse(string str)
        {
            return ParseTable.Get(str);
        }
        private static T ParseImpl(string str)
        {
            T type;
            if (!Enum.TryParse(str.Replace("_", string.Empty), true, out type))
            {
                throw new ArgumentException(string.Format("Can't parse {0} as {1}", str, typeof(T).Name));
            }
            return type;
        }

        public static string GetJavaName(T value)
        {
            return JavaNamesTable.Get(value);
        }

        static readonly Regex HumpExp = new Regex("([a-z])([A-Z0-9])", RegexOptions.Compiled);
        private static string GetJavaNameImpl(T value)
        {
            var netName = value.ToString();
            var javaName = HumpExp.Replace(netName, "$1_$2").ToUpperInvariant();
            return javaName;
        }
        #endregion

        #region BuilderBase
        public EnumBuilder(FudgeContext context, Type type)
            : base(context, type)
        {
        }

        public override object DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return Parse(msg.GetString(1));
        }

        protected override void SerializeImpl(object obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            serializer.WriteTypeHeader(msg, typeof(T));
            msg.Add(1, GetJavaName((T) obj));
        }

        #endregion
    }
}
