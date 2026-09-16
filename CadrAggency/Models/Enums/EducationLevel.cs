using System.ComponentModel;

namespace CadrAggency.Models.Enums
{
    /// <summary>
    /// Уровень образования соискателя.
    /// </summary>
    public enum EducationLevel
    {
        [Description("Высшее")]
        Higher,

        [Description("Среднее профессиональное")]
        SecondaryProfessional,

        [Description("Среднее")]
        Secondary,

        [Description("Неполное высшее")]
        IncompleteHigher
    }
}

