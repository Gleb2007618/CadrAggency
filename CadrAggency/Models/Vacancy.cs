using CadrAggency.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CadrAggency.Models
{
    /// <summary>
    /// Вакансия, размещённая кадровым агентством или компанией-клиентом.
    /// Содержит описание, требования и предлагаемую зарплату.
    /// </summary>
    [Index(nameof(Title), IsUnique = true)]
    public class Vacancy
    {
        /// <summary>Уникальный идентификатор.</summary>
        public int Id { get; set; }

        /// <summary>Название вакансии (уникальное).</summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        /// <summary>Описание обязанностей и условий работы.</summary>
        [Required]
        [MaxLength(4000)]
        public string Description { get; set; }

        /// <summary>Требования к кандидату.</summary>
        [Required]
        [MaxLength(4000)]
        public string Requirements { get; set; }

        /// <summary>Предлагаемая заработная плата.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }

        /// <summary>Текущий статус вакансии.</summary>
        public VacancyStatus Status { get; set; }

        /// <summary>Дата размещения вакансии.</summary>
        public DateTime PostedDate { get; set; }

        /// <summary>Дата последнего обновления записи.</summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>Список собеседований, назначенных по вакансии.</summary>
        public ICollection<Interview> Interviews { get; set; } = new List<Interview>();
    }
}


