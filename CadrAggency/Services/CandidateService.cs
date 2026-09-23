using CadrAggency.Data;
using CadrAggency.Models;
using CadrAggency.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CadrAggency.Services
{
    /// <summary>
    /// Сервис для работы с кандидатами.
    /// Инкапсулирует все запросы к базе данных, связанные с сущностью Candidate.
    /// </summary>
    public class CandidateService
    {
        private readonly HrDbContext _context;

        /// <summary>
        /// Конструктор сервиса.
        /// </summary>
        /// <param_name="context">Контекст базы данных.</param>
        public CandidateService(HrDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Возвращает кандидата по его идентификатору.
        /// </summary>
        /// <param_name="id">Идентификатор кандидата.</param>
        /// <returns>Кандидат или null, если не найден.</returns>
        public async Task<Candidate?> GetCandidateByIdAsync(int id)
        {
            return await _context.Candidates.FindAsync(id);
        }

        /// <summary>
        /// Возвращает список всех кандидатов без фильтрации.
        /// </summary>
        /// <returns>Список кандидатов.</returns>
        public async Task<List<Candidate>> GetAllCandidatesAsync()
        {
            return await _context.Candidates
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Возвращает список кандидатов с учётом фильтров и поиска.
        /// Фильтры по статусу, образованию и зарплате выполняются на стороне БД.
        /// Текстовый поиск выполняется в памяти, так как SQLite не поддерживает
        /// регистронезависимый поиск по кириллице средствами SQL.
        /// </summary>
        /// <param_name="searchTerm">Поисковая строка (имя, навыки, опыт).</param>
        /// <param_name="status">Фильтр по статусу кандидата.</param>
        /// <param_name="education">Фильтр по уровню образования.</param>
        /// <param_name="minSalary">Минимальная ожидаемая зарплата.</param>
        /// <param_name="maxSalary">Максимальная ожидаемая зарплата.</param>
        /// <returns>Отфильтрованный список кандидатов.</returns>
        public async Task<List<Candidate>> GetFilteredCandidatesAsync(
            string? searchTerm,
            CandidateStatus? status,
            EducationLevel? education,
            decimal? minSalary,
            decimal? maxSalary)
        {
            var query = _context.Candidates.AsNoTracking().AsQueryable();

            // Фильтры_на_стороне БД
            if (status.HasValue) query = query.Where(c => c.Status == status.Value);
            if (education.HasValue) query = query.Where(c => c.Education == education.Value);
            if (minSalary.HasValue) query = query.Where(c => c.ExpectedSalary >= minSalary.Value);
            if (maxSalary.HasValue) query = query.Where(c => c.ExpectedSalary <= maxSalary.Value);

            var list = await query.ToListAsync();

            // Текстовый_поиск — в памяти (регистронезависимый, работает с кириллицей)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                list = list.Where(c =>
                    (c.FullName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Skills?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    c.ExperienceYears.ToString().Contains(term)
                ).ToList();
            }

            return list;
        }

        /// <summary>
        /// Добавляет нового кандидата в базу данных.
        /// </summary>
        /// <param_name="candidate">Объект кандидата для добавления.</param>
        public async Task AddCandidateAsync(Candidate candidate)
        {
            _context.Candidates.Add(candidate);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Обновляет существующего кандидата в базе данных.
        /// Использует новый контекст и подгружает запись заново,
        /// чтобы избежать конфликта отслеживания сущностей.
        /// </summary>
        /// <param_name="candidate">Объект кандидата с новыми данными.</param>
        public async Task UpdateCandidateAsync(Candidate candidate)
        {
            using var context = new HrDbContext();
            var existing = await context.Candidates.FindAsync(candidate.Id);
            if (existing != null)
            {
                existing.FullName = candidate.FullName;
                existing.Telephone = candidate.Telephone;
                existing.Email = candidate.Email;
                existing.BirthDate = candidate.BirthDate;
                existing.Education = candidate.Education;
                existing.Skills = candidate.Skills;
                existing.ExperienceYears = candidate.ExperienceYears;
                existing.ExpectedSalary = candidate.ExpectedSalary;
                existing.Status = candidate.Status;
                existing.ResumeText = candidate.ResumeText;
                existing.ResumeFilePath = candidate.ResumeFilePath;
                existing.LastUpdated = candidate.LastUpdated;
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Удаляет кандидата по идентификатору.
        /// </summary>
        /// <param_name="id">Идентификатор кандидата.</param>
        public async Task DeleteCandidateAsync(int id)
        {
            var candidate = await _context.Candidates.FindAsync(id);
            if (candidate != null)
            {
                _context.Candidates.Remove(candidate);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Возвращает общее количество кандидатов в базе.
        /// </summary>
        /// <returns>Количество кандидатов.</returns>
        public async Task<int> GetTotalCandidatesCountAsync()
        {
            return await _context.Candidates.CountAsync();
        }

        /// <summary>
        /// Возвращает количество активных (открытых) вакансий.
        /// Используется для карточки аналитики.
        /// </summary>
        /// <returns>Количество вакансий со статусом Open.</returns>
        public async Task<int> GetActiveVacanciesCountAsync()
        {
            return await _context.Vacancies.CountAsync(v => v.Status == VacancyStatus.Open);
        }

        /// <summary>
        /// Возвращает количество собеседований на текущей неделе.
        /// </summary>
        /// <returns>Число собеседований за текущую неделю.</returns>
        public async Task<int> GetWeeklyInterviewsCountAsync()
        {
            var now = DateTime.UtcNow;
            var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);
            return await _context.Interviews.CountAsync(i =>
                i.ScheduledDateTime >= startOfWeek && i.ScheduledDateTime < endOfWeek);
        }

        /// <summary>
        /// Возвращает количество успешных собеседований за последний месяц.
        /// </summary>
        /// <returns>Число успешных собеседований.</returns>
        public async Task<int> GetMonthlySuccessfulInterviewsCountAsync()
        {
            var startOfMonth = DateTime.UtcNow.AddMonths(-1);
            return await _context.Interviews.CountAsync(i =>
                i.Result == InterviewResult.Successful && i.ScheduledDateTime >= startOfMonth);
        }

        /// <summary>
        /// Проверяет, существует ли кандидат с указанным полным именем.
        /// Используется для валидации уникальности имени.
        /// </summary>
        /// <param_name="fullName">Полное имя для проверки.</param>
        /// <param_name="excludeId">ID для исключения (при редактировании).</param>
        /// <returns>True, если кандидат с таким именем уже существует.</returns>
        public async Task<bool> IsFullNameExistsAsync(string fullName, int? excludeId = null)
        {
            var query = _context.Candidates.Where(c => c.FullName == fullName);
            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        /// <summary>
        /// Проверяет, есть ли кандидат с такой же парой имя+телефон.
        /// Используется при импорте CSV для пропуска дубликатов.
        /// </summary>
        /// <param_name="fullName">Имя кандидата.</param>
        /// <param_name="telephone">Телефон кандидата.</param>
        /// <returns>True, если найден дубликат.</returns>
        public async Task<bool> IsDuplicateAsync(string fullName, string telephone)
        {
            return await _context.Candidates
                .AnyAsync(c => c.FullName == fullName && c.Telephone == telephone);
        }
    }
}

