using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CadrAggency.Data;
using CadrAggency.Models;
using CadrAggency.Models.Enums;

namespace CadrAggency.Services
{
    /// <summary>
    /// Сервис для работы с собеседованиями.
    /// Инкапсулирует все запросы к базе данных, связанные с сущностью Interview.
    /// </summary>
    public class InterviewService
    {
        private readonly HrDbContext _context;

        /// <summary>
        /// Конструктор сервиса.
        /// </summary>
        /// <param_name="context">Контекст базы данных.</param>
        public InterviewService(HrDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Возвращает список всех собеседований с подгруженными данными
        /// о соискателе и вакансии, отсортированный по дате (свежие сверху).
        /// </summary>
        /// <returns>Список собеседований.</returns>
        public async Task<List<Interview>> GetAllInterviewsAsync()
        {
            return await _context.Interviews
                .AsNoTracking()
                .Include(i => i.Candidate)
                .Include(i => i.Vacancy)
                .OrderByDescending(i => i.ScheduledDateTime)
                .ToListAsync();
        }

        /// <summary>
        /// Возвращает список собеседований с учётом фильтров.
        /// Любой параметр может быть null — тогда фильтр не применяется.
        /// </summary>
        /// <param_name="candidateId">Фильтр по идентификатору соискателя.</param>
        /// <param_name="vacancyId">Фильтр по идентификатору вакансии.</param>
        /// <param_name="result">Фильтр по результату собеседования.</param>
        /// <returns>Отфильтрованный список собеседований.</returns>
        public async Task<List<Interview>> GetFilteredInterviewsAsync(
            int? candidateId,
            int? vacancyId,
            InterviewResult? result)
        {
            var query = _context.Interviews
                .AsNoTracking()
                .Include(i => i.Candidate)
                .Include(i => i.Vacancy)
                .AsQueryable();

            if (candidateId.HasValue)
                query = query.Where(i => i.CandidateId == candidateId.Value);

            if (vacancyId.HasValue)
                query = query.Where(i => i.VacancyId == vacancyId.Value);

            if (result.HasValue)
                query = query.Where(i => i.Result == result.Value);

            return await query
                .OrderByDescending(i => i.ScheduledDateTime)
                .ToListAsync();
        }

        /// <summary>
        /// Добавляет новое собеседование в базу данных.
        /// </summary>
        /// <param_name="interview">Объект собеседования для добавления.</param>
        public async Task AddInterviewAsync(Interview interview)
        {
            _context.Interviews.Add(interview);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
        }

        /// <summary>
        /// Обновляет существующее собеседование в базе данных.
        /// Использует новый контекст и подгружает запись заново,
        /// чтобы избежать конфликта отслеживания сущностей.
        /// </summary>
        /// <param_name="interview">Объект собеседования с новыми данными.</param>
        public async Task UpdateInterviewAsync(Interview interview)
        {
            using var context = new HrDbContext();
            var existing = await context.Interviews.FindAsync(interview.Id);
            if (existing != null)
            {
                existing.Result = interview.Result;
                existing.Comment = interview.Comment;
                existing.Format = interview.Format;
                existing.ScheduledDateTime = interview.ScheduledDateTime;
                existing.CandidateId = interview.CandidateId;
                existing.VacancyId = interview.VacancyId;
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Удаляет собеседование по идентификатору.
        /// </summary>
        /// <param_name="id">Идентификатор собеседования.</param>
        public async Task DeleteInterviewAsync(int id)
        {
            var interview = await _context.Interviews.FindAsync(id);
            if (interview != null)
            {
                _context.Interviews.Remove(interview);
                await _context.SaveChangesAsync();
                _context.ChangeTracker.Clear();
            }
        }

        /// <summary>
        /// Возвращает список активных соискателей.
        /// Используется для выпадающего списка при назначении собеседования.
        /// </summary>
        /// <returns>Список соискателей со статусом Active.</returns>
        public async Task<List<Candidate>> GetActiveCandidatesAsync()
        {
            return await _context.Candidates
                .AsNoTracking()
                .Where(c => c.Status == CandidateStatus.Active)
                .OrderBy(c => c.FullName)
                .ToListAsync();
        }

        /// <summary>
        /// Возвращает список открытых вакансий.
        /// Используется для выпадающего списка при назначении собеседования.
        /// </summary>
        /// <returns>Список вакансий со статусом Open.</returns>
        public async Task<List<Vacancy>> GetOpenVacanciesAsync()
        {
            return await _context.Vacancies
                .AsNoTracking()
                .Where(v => v.Status == VacancyStatus.Open)
                .OrderBy(v => v.Title)
                .ToListAsync();
        }

        /// <summary>
        /// Возвращает всех соискателей для фильтра таблицы собеседований.
        /// </summary>
        /// <returns>Список всех соискателей, отсортированный по имени.</returns>
        public async Task<List<Candidate>> GetAllCandidatesForFilterAsync()
        {
            return await _context.Candidates
                .AsNoTracking()
                .OrderBy(c => c.FullName)
                .ToListAsync();
        }

        /// <summary>
        /// Возвращает все вакансии для фильтра таблицы собеседований.
        /// </summary>
        /// <returns>Список всех вакансий, отсортированный по названию.</returns>
        public async Task<List<Vacancy>> GetAllVacanciesForFilterAsync()
        {
            return await _context.Vacancies
                .AsNoTracking()
                .OrderBy(v => v.Title)
                .ToListAsync();
        }
    }
}


