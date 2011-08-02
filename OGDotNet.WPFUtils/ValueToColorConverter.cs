//-----------------------------------------------------------------------
// <copyright file="ValueToColorConverter.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace OGDotNet.WPFUtils
{
    public class ValueToColorConverter : IValueConverter
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

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && !(value is double))
                throw new NotImplementedException();

            if (targetType == typeof(Color))
            {
                if (value == null)
                    return Colors.White;
                var colorQuotient = (double)value;
                return GetColor(colorQuotient / _range);
            }

            if (targetType == typeof(Brush))
            {
                var color = (Color)Convert(value, typeof(Color), parameter, culture);
                return new SolidColorBrush(color);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static Color GetColor(double colorQuotientDouble, Color minColor, Color maxColor)
        {
            return new ValueToColorConverter(1, minColor, maxColor).GetColor(colorQuotientDouble);
        }

        public Color GetColor(double colorQuotientDouble)
        {
            var colorQuotient = (float)colorQuotientDouble;
            return _minColor * (1 - colorQuotient) + _maxColor * colorQuotient;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
