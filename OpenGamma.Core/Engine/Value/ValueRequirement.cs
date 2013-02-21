// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValueRequirement.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge;
using Fudge.Serialization;
using OpenGamma.Engine.Target;
using OpenGamma.Id;

namespace OpenGamma.Engine.Value
{
    public class ValueRequirement
    {
        private readonly string _valueName;
        private readonly ComputationTargetReference _targetReference;
        private readonly ValueProperties _constraints;

        public ValueRequirement(string valueName, ComputationTargetReference targetReference) : this(valueName, targetReference, ValueProperties.Create())
        {
        }

        public ValueRequirement(string valueName, ComputationTargetReference targetReference, ValueProperties constraints)
        {
            _valueName = string.Intern(valueName); // Should be small static set
            _constraints = constraints;
            _targetReference = targetReference;
        }

        public string ValueName { get { return _valueName; } }
        public ValueProperties Constraints { get { return _constraints; } }
        public ComputationTargetReference TargetReference { get { return _targetReference; } }

        public bool IsSatisfiedBy(ValueSpecification valueSpecification) {
            if (ValueName != valueSpecification.ValueName) {
              return false;
            }

            if (!TargetReference.Equals(valueSpecification.TargetSpecification)) {
              return false;
            }

            if (!Constraints.IsSatisfiedBy(valueSpecification.Properties)) {
              return false;
            }

            return true;
        }

        public static UniqueId GetUniqueIdentifier(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer, string fieldName)
        {
            UniqueId portfolioIdentifier;
            IFudgeField idField = ffc.GetByName(fieldName);
            if (idField != null)
            {
                var value = idField.Value;
                if (value is string)
                {
                    portfolioIdentifier = UniqueId.Parse((string) value);
                }
                else if (value is IFudgeFieldContainer)
                {
                    portfolioIdentifier = deserializer.FromField<UniqueId>(idField);
                }
                else
                {
                    throw new ArgumentException(string.Format("Couldn't read UID {0}", value));
                }
            }
            else
            {
                portfolioIdentifier = null;
            }

            return portfolioIdentifier;
        }
    }
}