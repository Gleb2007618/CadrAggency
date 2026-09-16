using CadrAggency.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace CadrAggency.Models
{
    /// <summary>
    /// Собеседование — связывает конкретного соискателя
    /// с конкретной вакансией и фиксирует результат встречи.
    /// </summary>
    public class Interview
    {
        /// <summary>Уникальный идентификатор.</summary>
        public int Id { get; set; }

        /// <summary>Идентификатор соискателя.</summary>
        [Required]
        public int CandidateId { get; set; }

        /// <summary>Идентификатор вакансии.</summary>
        [Required]
        public int VacancyId { get; set; }

        /// <summary>Дата и время начала собеседования.</summary>
        [Required]
        public DateTime ScheduledDateTime { get; set; }

        /// <summary>Формат собеседования (очное, онлайн, телефонное).</summary>
        [Required]
        public InterviewFormat Format { get; set; }

        /// <summary>Результат собеседования (заполняется после завершения).</summary>
        public InterviewResult? Result { get; set; }

        /// <summary>Комментарий рекрутера по итогам собеседования.</summary>
        [MaxLength(4000)]
        public string? Comment { get; set; }

        /// <summary>Навигационное свойство — соискатель.</summary>
        public Candidate Candidate { get; set; }

        /// <summary>Навигационное свойство — вакансия.</summary>
        public Vacancy Vacancy { get; set; }
    }
}


