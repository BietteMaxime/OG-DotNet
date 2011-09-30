//-----------------------------------------------------------------------
// <copyright file="DoubleToColorConverterBase.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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
    public abstract class DoubleToColorConverterBase : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && !(value is double))
                return null;

            if (targetType == typeof(Color))
            {
                if (value == null)
                    return Colors.White;
                return GetColor((double)value);
            }

            if (targetType == typeof(Brush))
            {
                var color = (Color)Convert(value, typeof(Color), parameter, culture);
                return new SolidColorBrush(color);
            }
            else
            {
                return null;
            }
        }

        public abstract Color GetColor(double value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}