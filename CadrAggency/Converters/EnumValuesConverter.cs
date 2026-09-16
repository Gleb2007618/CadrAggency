using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace CadrAggency.Converters
{
    /// <summary>
    /// Конвертер типа enum в список его значений.
    /// Используется для наполнения ComboBox всеми значениями перечисления.
    /// </summary>
    public class EnumValuesConverter : IValueConverter
    {
        /// <summary>
        /// Возвращает массив всех значений указанного enum.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Type enumType && enumType.IsEnum)
                return Enum.GetValues(enumType);
            return new List<object>();
        }

        /// <summary>Обратное преобразование не поддерживается.</summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

