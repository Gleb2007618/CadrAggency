using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using CadrAggency.Models.Enums;

namespace CadrAggency.Converters
{
    /// <summary>
    /// Конвертер статуса вакансии в цвет фона плашки.
    /// Открыта — зелёный, на рассмотрении — жёлтый, закрыта — серый,
    /// приостановлена — красноватый.
    /// </summary>
    public class VacancyStatusToBrushConverter : IValueConverter
    {
        /// <summary>Единственный экземпляр конвертера для использования в XAML.</summary>
        public static VacancyStatusToBrushConverter Instance { get; } = new();

        /// <summary>
        /// Возвращает цвет фона для указанного статуса вакансии.
        /// </summary>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is VacancyStatus status)
            {
                return status switch
                {
                    VacancyStatus.Open => new SolidColorBrush(Colors.Green),
                    VacancyStatus.Pending => new SolidColorBrush(Colors.Gold),
                    VacancyStatus.Closed => new SolidColorBrush(Colors.Gray),
                    VacancyStatus.Suspended => new SolidColorBrush(Colors.IndianRed),
                    _ => new SolidColorBrush(Colors.LightGray)
                };
            }
            return new SolidColorBrush(Colors.Red);
        }

        /// <summary>Обратное преобразование не поддерживается.</summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

