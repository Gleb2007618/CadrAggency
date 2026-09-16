using System.ComponentModel;

namespace CadrAggency.Models.Enums
{
    /// <summary>
    /// Статус соискателя в системе.
    /// </summary>
    public enum CandidateStatus
    {
        [Description("Активен")]
        Active,

        [Description("На рассмотрении")]
        Pending,

        [Description("Трудоустроен")]
        Employed,

        [Description("Неактивен")]
        Inactive
    }
}

