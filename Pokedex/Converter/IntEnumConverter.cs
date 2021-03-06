﻿using System;
using System.Globalization;
using Xamarin.Forms;

namespace Pokedex.Converter
{
    public class IntEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter = null, CultureInfo culture = null)
        {
            if (value is Enum)
            {
                return (int)value;
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter = null, CultureInfo culture = null)
        {
            if (value is int)
            {
                return Enum.ToObject(targetType, value);
            }
            return Binding.DoNothing;
        }
    }
}
