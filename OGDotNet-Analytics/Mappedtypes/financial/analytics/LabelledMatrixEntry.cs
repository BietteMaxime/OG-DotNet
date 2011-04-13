//-----------------------------------------------------------------------
// <copyright file="LabelledMatrixEntry.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

namespace OGDotNet.Mappedtypes.financial.analytics
{
    public class LabelledMatrixEntry
    {
        private readonly object _label;
        private readonly double _value;

        public LabelledMatrixEntry(object label, double value)
        {
            _label = label;
            _value = value;
        }

        public object Label
        {
            get { return _label; }
        }

        public double Value
        {
            get { return _value; }
        }
    }
}