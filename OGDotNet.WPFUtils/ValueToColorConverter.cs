// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValueToColorConverter.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Windows.Media;

namespace OGDotNet.WPFUtils
{
    public class ValueToColorConverter : DoubleToColorConverterBase
    {
        private static readonly Color DefaultMinColor = Colors.Yellow;
        private static readonly Color DefaultMaxColor = Colors.Red;
        private readonly Color _minColor;
        private readonly Color _maxColor;
        private readonly double _range;

        public ValueToColorConverter(double range)
            : this(range, DefaultMinColor, DefaultMaxColor)
        {
        }

        public ValueToColorConverter(double range, Color minColor, Color maxColor)
        {
            _range = range;
            _minColor = minColor;
            _maxColor = maxColor;
        }

        public static Color GetColor(double colorQuotientDouble, Color minColor, Color maxColor)
        {
            return new ValueToColorConverter(1, minColor, maxColor).GetColor(colorQuotientDouble);
        }

        public override Color GetColor(double value)
        {
            var colorQuotient = (float)(value / _range);
            return _minColor * (1 - colorQuotient) + _maxColor * colorQuotient;
        }
    }
}
