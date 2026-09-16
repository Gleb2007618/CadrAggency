using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using CadrAggency.Models;

namespace CadrAggency.Data
{
    /// <summary>
    /// Контекст базы данных приложения.
    /// Управляет подключением к БД и предоставляет доступ к таблицам
    /// кандидатов, вакансий и собеседований.
    /// </summary>
    public class HrDbContext : DbContext
    {
        /// <summary>Таблица кандидатов.</summary>
        public DbSet<Candidate> Candidates { get; set; }

        /// <summary>Таблица вакансий.</summary>
        public DbSet<Vacancy> Vacancies { get; set; }

        /// <summary>Таблица собеседований.</summary>
        public DbSet<Interview> Interviews { get; set; }

        /// <summary>
        /// Настраивает подключение к базе данных.
        /// Провайдер можно заменить на PostgreSQL без изменения моделей.
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=HrAgency.db");
        }

        /// <summary>
        /// Настройка связей между сущностями и параметров полей.
        /// Задаёт каскадное удаление для собеседований и точность
        /// денежных полей.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Interview>()
            .HasOne(i => i.Candidate)
            .WithMany(c => c.Interviews)
            .HasForeignKey(i => i.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Interview>()
            .HasOne(i => i.Vacancy)
            .WithMany(v => v.Interviews)
            .HasForeignKey(i => i.VacancyId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Candidate>()
            .Property(c => c.ExpectedSalary)
            .HasPrecision(18, 2);

            modelBuilder.Entity<Vacancy>()
            .Property(v => v.Salary)
            .HasPrecision(18, 2);
        }
    }
}

