using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using CadrAggency.Models.Enums;

namespace CadrAggency.Converters
{
    /// <summary>
    /// Конвертер статуса кандидата в цвет фона плашки.
    /// Активен — зелёный, на рассмотрении — жёлтый, трудоустроен — синий,
    /// неактивен — серый.
    /// </summary>
    public class StatusToBrushConverter : IValueConverter
    {
        /// <summary>Единственный экземпляр конвертера для использования в XAML.</summary>
        public static StatusToBrushConverter Instance { get; } = new();

        /// <summary>
        /// Возвращает цвет фона для указанного статуса кандидата.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CandidateStatus status)
            {
                return status switch
                {
                    CandidateStatus.Active => new SolidColorBrush(Colors.Green),
                    CandidateStatus.Pending => new SolidColorBrush(Colors.Gold),
                    CandidateStatus.Employed => new SolidColorBrush(Colors.DodgerBlue),
                    CandidateStatus.Inactive => new SolidColorBrush(Colors.Gray),
                    _ => new SolidColorBrush(Colors.Transparent)
                };
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        /// <summary>Обратное преобразование не поддерживается.</summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

