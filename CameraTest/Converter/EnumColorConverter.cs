using System;
using System.Globalization;
using Xamarin.Forms;

namespace CameraTest.Converter
{
    public class EnumColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter = null, CultureInfo culture = null)
        {
            if (value is Enum)
            {
                string s = value.ToString();
                return (Color)Application.Current.Resources[value.ToString()];
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter = null, CultureInfo culture = null)
        {
            //if (value is int)
            //{
            //    return Enum.ToObject(targetType, value);
            //}
            return Binding.DoNothing;
        }
    }
}
