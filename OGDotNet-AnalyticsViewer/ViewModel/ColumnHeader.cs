// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ColumnHeader.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenGamma.Engine.value;

namespace OGDotNet.AnalyticsViewer.ViewModel
{
    public class ColumnHeader : IEquatable<ColumnHeader>
    {
        private readonly string _configuration;
        private readonly string _valueName;
        private readonly ValueProperties _requiredConstraints;

        public ColumnHeader(string configuration, string valueName, ValueProperties requiredConstraints)
        {
            _configuration = configuration;
            _valueName = valueName;

            _requiredConstraints = requiredConstraints;
        }

        public override string ToString()
        {
            return Text;
        }

        public string Text
        {
            get { return _configuration == "Default" ? _valueName : string.Format("{0}/{1}", _configuration, _valueName); }
        }
        public string ToolTip
        {
            get { return GetPropertiesString(_requiredConstraints); }
        }
        public ValueProperties RequiredConstraints
        {
            get { return _requiredConstraints; }
        }

        public string Configuration
        {
            get { return _configuration; }
        }

        public string ValueName
        {
            get { return _valueName; }
        }

        private static string GetPropertiesString(ValueProperties constraints)
        {
            if (constraints.IsEmpty)
            {
                return "No constraints";
            }

            var sb = new StringBuilder();
            bool firstProperty = true;
            foreach (string propertyName in constraints.Properties)
            {
                if (propertyName == "Function")
                {
                    continue;
                }

                if (firstProperty)
                {
                    firstProperty = false;
                }
                else
                {
                    sb.Append("; \n");
                }

                sb.Append(propertyName).Append("=");
                ISet<string> propertyValues = constraints.GetValues(propertyName);
                if (propertyValues.Count() == 0)
                {
                    sb.Append("[empty]");
                }
                else if (propertyValues.Count() == 1)
                {
                    sb.Append(propertyValues.Single());
                }
                else
                {
                    sb.Append("(");
                    sb.Append(string.Join(", ", propertyValues));
                    sb.Append(")");
                }
            }

            return sb.ToString();
        }

        public bool Equals(ColumnHeader other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._configuration, _configuration) && Equals(other._valueName, _valueName) && Equals(other._requiredConstraints, _requiredConstraints);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ColumnHeader)) return false;
            return Equals((ColumnHeader)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = _configuration.GetHashCode();
                result = (result * 397) ^ _valueName.GetHashCode();
                result = (result * 397) ^ _requiredConstraints.GetHashCode();
                return result;
            }
        }
    }
}