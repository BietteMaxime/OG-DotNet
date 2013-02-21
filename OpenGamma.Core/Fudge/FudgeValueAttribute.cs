// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FudgeValueAttribute.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using OpenGamma.Util;

namespace OpenGamma.Fudge
{
    [AttributeUsage(AttributeTargets.Field)]
    public class FudgeValueAttribute : Attribute
    {
        private string _value;

        public FudgeValueAttribute(string value)
        {
            Value = value;
        }

        public string Value {
            get
            {
                return _value;
            }
            set
            {
                ArgumentChecker.NotNull(value, "value");
                _value = value;
            }
        }
    }
}