﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

using Avalonia.Data.Converters;

namespace BEditor.Converters
{
    public class EnumerableSkipConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<object> f && int.TryParse(parameter.ToString(), out var count))
            {
                return f.Skip(count);
            }

            return value;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
