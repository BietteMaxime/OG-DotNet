// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DoubleMatrix1D.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenGamma.Analytics.Math.Matrix
{
    public class DoubleMatrix1D
    {
        private readonly double[] _data;

        public DoubleMatrix1D(double[] data)
        {
            _data = data;
        }

        public double[] Data
        {
            get { return _data; }
        }
    }
}
