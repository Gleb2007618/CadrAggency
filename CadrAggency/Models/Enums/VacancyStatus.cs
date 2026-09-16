using System.ComponentModel;

namespace CadrAggency.Models.Enums
{
    /// <summary>
    /// Статус вакансии.
    /// </summary>
    public enum VacancyStatus
    {
        [Description("Открыта")]
        Open,

        [Description("На рассмотрении")]
        Pending,

        [Description("Закрыта")]
        Closed,

        [Description("Приостановлена")]
        Suspended
    }
}

