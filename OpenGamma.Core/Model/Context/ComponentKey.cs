// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComponentKey.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace OpenGamma.Model.Context
{
    public class ComponentKey : IEquatable<ComponentKey>
    {
        private readonly string _type;
        private readonly string _classifier;

        public ComponentKey(string type, string classifier)
        {
            _type = type;
            _classifier = classifier;
        }

        public string Type
        {
            get { return _type; }
        }

        public string Classifier
        {
            get { return _classifier; }
        }

        public bool Equals(ComponentKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._type, _type) && Equals(other._classifier, _classifier);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ComponentKey)) return false;
            return Equals((ComponentKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_type.GetHashCode() * 397) ^ _classifier.GetHashCode();
            }
        }

        public static bool operator ==(ComponentKey left, ComponentKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ComponentKey left, ComponentKey right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return string.Format("[ComponentKey {0}/{1}]", _type, _classifier);
        }
    }
}