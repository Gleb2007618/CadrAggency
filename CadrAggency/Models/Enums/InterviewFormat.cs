using System.ComponentModel;

namespace CadrAggency.Models.Enums
{
    /// <summary>
    /// Формат проведения собеседования.
    /// </summary>
    public enum InterviewFormat
    {
        [Description("Очное")]
        Offline,

        [Description("Онлайн")]
        Online,

        [Description("Телефонное")]
        Phone
    }
}


