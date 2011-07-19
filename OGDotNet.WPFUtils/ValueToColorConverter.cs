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
        private readonly double _range;

        public ValueToColorConverter(double range)
        {
            _range = range;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double))
                throw new NotImplementedException();

            if (targetType == typeof(Color))
            {
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

        public static Color GetColor(double colorQuotientDouble)
        {
            var colorQuotient = (float)colorQuotientDouble;
            return Colors.Yellow * (1 - colorQuotient) + Colors.Red * colorQuotient;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
