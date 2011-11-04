//-----------------------------------------------------------------------
// <copyright file="DoubleLabelledMatrix2D.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.Financial.Analytics
{
    [FudgeSurrogate(typeof(DoubleLabelledMatrix2DBuilder))]
    public class DoubleLabelledMatrix2D : IEnumerable<LabelledMatrixEntry2D>
    {
        private readonly IList<double> _xKeys;
        private readonly IList<double> _yKeys;
        private readonly IList<object> _xLabels;
        private readonly IList<object> _yLabels;
        private readonly double[][] _values;

        public DoubleLabelledMatrix2D(IList<double> xKeys, IList<double> yKeys, IList<object> xLabels, IList<object> yLabels, double[][] values)
        {
            ArgumentChecker.NotNull(xKeys, "xKeys");
            ArgumentChecker.NotNull(xLabels, "xLabels");
            ArgumentChecker.NotNull(yKeys, "yKeys");
            ArgumentChecker.NotNull(yLabels, "yLabels");
            ArgumentChecker.NotNull(values, "values");

            if (values.Length != yKeys.Count)
            {
                throw new ArgumentException("Labelled matrix is the wrong shape");
            }
            if (values.Any(r => r.Length != xKeys.Count))
            {
                throw new ArgumentException("Labelled matrix is the wrong shape");
            }
            _xKeys = xKeys;
            _yKeys = yKeys;
            _xLabels = xLabels;
            _yLabels = yLabels;
            _values = values;
        }

        public IList<double> XKeys
        {
            get { return _xKeys; }
        }

        public IList<double> YKeys
        {
            get { return _yKeys; }
        }

        public IList<object> XLabels
        {
            get { return _xLabels; }
        }

        public IList<object> YLabels
        {
            get { return _yLabels; }
        }

        public double[][] Values
        {
            get { return _values; }
        }
       
        private IEnumerable<LabelledMatrixEntry2D> GetEntrys()
        {
            for (int xIndex = 0; xIndex < XLabels.Count; xIndex++)
            {
                var xLabel = XLabels[xIndex];
                var xKey = XKeys[xIndex];
                for (int yIndex = 0; yIndex < YLabels.Count; yIndex++)
                {
                    var yLabel = YLabels[yIndex];
                    var yKey = YKeys[yIndex];
                    yield return new LabelledMatrixEntry2D(xLabel, yLabel, xKey, yKey, Values[yIndex][xIndex]);
                }
            }
        }

        public IEnumerator<LabelledMatrixEntry2D> GetEnumerator()
        {
            return GetEntrys().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
