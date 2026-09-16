using CadrAggency.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CadrAggency.Models
{
    /// <summary>
    /// Соискатель (кандидат) — центральная сущность системы.
    /// Хранит персональные данные, навыки, ожидания по зарплате
    /// и текущий статус.
    /// </summary>
    [Index(nameof(FullName), IsUnique = true)]
    public class Candidate
    {
        /// <summary>Уникальный идентификатор.</summary>
        public int Id { get; set; }

        /// <summary>Полное имя (уникальное).</summary>
        [Required]
        [MaxLength(200)]
        public string FullName { get; set; }

        /// <summary>Контактный телефон.</summary>
        [Required]
        [MaxLength(20)]
        public string Telephone { get; set; }

        /// <summary>Адрес электронной почты.</summary>
        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        /// <summary>Дата рождения.</summary>
        public DateTime BirthDate { get; set; }

        /// <summary>Уровень образования.</summary>
        public EducationLevel Education { get; set; }

        /// <summary>Перечень навыков (через запятую).</summary>
        [MaxLength(1000)]
        public string Skills { get; set; }

        /// <summary>Опыт работы в годах.</summary>
        public int ExperienceYears { get; set; }

        /// <summary>Ожидаемая заработная плата.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal ExpectedSalary { get; set; }

        /// <summary>Текущий статус соискателя.</summary>
        public CandidateStatus Status { get; set; }

        /// <summary>Текст резюме (может отсутствовать).</summary>
        [MaxLength(4000)]
        public string? ResumeText { get; set; }

        /// <summary>Путь к прикреплённому файлу резюме.</summary>
        public string? ResumeFilePath { get; set; }

        /// <summary>Дата последнего обновления записи.</summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>Список собеседований соискателя.</summary>
        public ICollection<Interview> Interviews { get; set; } = new List<Interview>();
    }
}


