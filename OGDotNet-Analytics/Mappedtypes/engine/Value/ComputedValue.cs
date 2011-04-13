//-----------------------------------------------------------------------
// <copyright file="ComputedValue.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

namespace OGDotNet.Mappedtypes.engine.Value
{
    public class ComputedValue
    {
        private readonly ValueSpecification _specification;
        private readonly object _value;

        public ComputedValue(ValueSpecification specification, object value)
        {
            _specification = specification;
            _value = value;
        }

        public ValueSpecification Specification
        {
            get { return _specification; }
        }

        public object Value
        {
            get { return _value; }
        }
    }
}