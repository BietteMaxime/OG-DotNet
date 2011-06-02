//-----------------------------------------------------------------------
// <copyright file="ValueSpecification.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.engine.value
{
    [FudgeSurrogate(typeof(ValueSpecificationBuilder))]
    public class ValueSpecification : IEquatable<ValueSpecification>
    {
        private readonly string _valueName;
        private readonly ComputationTargetSpecification _targetSpecification;
        private readonly ValueProperties _properties;

        public ValueSpecification(string valueName, ComputationTargetSpecification targetSpecification, ValueProperties properties)
        {
            ArgumentChecker.NotNull(valueName, "valueName");
            ArgumentChecker.NotNull(targetSpecification, "targetSpecification");
            ArgumentChecker.NotNull(properties, "properties");
            _valueName = valueName;
            _targetSpecification = targetSpecification;
            _properties = properties;
        }

        public ValueProperties Properties
        {
            get { return _properties; }
        }

        public string ValueName
        {
            get { return _valueName; }
        }

        public ComputationTargetSpecification TargetSpecification
        {
            get { return _targetSpecification; }
        }

        public bool Equals(ValueSpecification other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._valueName, _valueName) && Equals(other._targetSpecification, _targetSpecification);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ValueSpecification)) return false;
            return Equals((ValueSpecification)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_valueName != null ? _valueName.GetHashCode() : 0) * 397) ^ (_targetSpecification != null ? _targetSpecification.GetHashCode() : 0);
            }
        }
    }
}