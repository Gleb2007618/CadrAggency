using System.ComponentModel;

namespace CadrAggency.Models.Enums
{
    /// <summary>
    /// Результат собеседования.
    /// </summary>
    public enum InterviewResult
    {
        [Description("Успешно")]
        Successful,

        [Description("Неуспешно")]
        Unsuccessful,

        [Description("Перенесено")]
        Postponed,

        [Description("Назначен следующий этап")]
        NextStage
    }
}

