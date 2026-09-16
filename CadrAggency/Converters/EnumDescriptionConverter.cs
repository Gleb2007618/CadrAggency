using System;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace CadrAggency.Converters
{
    /// <summary>
    /// Конвертер enum ↔ строка.
    /// Для отображения берёт русское описание из атрибута [Description],
    /// для обратного преобразования ищет enum по описанию.
    /// </summary>
    public class EnumDescriptionConverter : IValueConverter
    {
        /// <summary>Единственный экземпляр конвертера для использования в XAML.</summary>
        public static EnumDescriptionConverter Instance { get; } = new();

        /// <summary>
        /// Преобразует значение enum в его русское описание.
        /// </summary>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Enum e)
                return e.GetDescription();
            return value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Преобразует строку обратно в значение enum.
        /// Ищет совпадение по описанию или по имени.
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var enumType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (!enumType.IsEnum) return null;

            var stringValue = value?.ToString();
            if (string.IsNullOrEmpty(stringValue))
                return null;

            foreach (var field in enumType.GetFields())
            {
                var attr = field.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false)
                    .FirstOrDefault() as System.ComponentModel.DescriptionAttribute;
                if (attr?.Description == stringValue)
                    return Enum.Parse(enumType, field.Name);
            }

            if (Enum.TryParse(enumType, stringValue, out var result))
                return result;

            return null;
        }
    }
}


