using System;
using System.ComponentModel;
using System.Reflection;

namespace CadrAggency.Converters
{
    /// <summary>
    /// Расширения для работы с enum.
    /// Позволяет получить русское описание значения из атрибута [Description].
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Возвращает описание значения enum из атрибута [Description].
        /// Если атрибут отсутствует — возвращает имя значения.
        /// </summary>
        /// <param name="value">Значение enum.</param>
        /// <returns>Строковое описание.</returns>
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = field?.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? value.ToString();
        }
    }
}

