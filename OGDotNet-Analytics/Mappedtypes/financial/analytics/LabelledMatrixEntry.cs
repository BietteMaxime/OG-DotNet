//-----------------------------------------------------------------------
// <copyright file="LabelledMatrixEntry.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.financial.analytics
{
    public class LabelledMatrixEntry2D
    {
        private readonly object _xLabel;
        private readonly object _yLabel;
        private readonly double _value;

        public LabelledMatrixEntry2D(object xLabel, object yLabel, double value)
        {
            _xLabel = xLabel;
            _yLabel = yLabel;
            _value = value;
        }

        public object XLabel
        {
            get { return _xLabel; }
        }

        public object YLabel
        {
            get { return _yLabel; }
        }

        public double Value
        {
            get { return _value; }
        }

        public static explicit operator LabelledMatrixEntry(LabelledMatrixEntry2D f)
        {
            return new LabelledMatrixEntry(Tuple.Create(f.XLabel, f.YLabel), f.Value);
        }
    }

    public class LabelledMatrixEntry
    {
        private readonly object _label;
        private readonly double _value;

        public LabelledMatrixEntry(object label, double value)
        {
            ArgumentChecker.NotNull(label, "label");

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