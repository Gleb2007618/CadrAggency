using CadrAggency.Data;
using CadrAggency.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CadrAggency.Services
{
    /// <summary>
    /// Сервис для работы с вакансиями.
    /// Инкапсулирует все запросы к базе данных, связанные с сущностью Vacancy.
    /// </summary>
    public class VacancyService
    {
        private readonly HrDbContext _context;

        /// <summary>
        /// Конструктор сервиса.
        /// </summary>
        /// <param_name="context">Контекст базы данных.</param>
        public VacancyService(HrDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Возвращает список всех вакансий без фильтрации.
        /// </summary>
        /// <returns>Список вакансий.</returns>
        public async Task<List<Vacancy>> GetAllVacanciesAsync()
        {
            return await _context.Vacancies
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Добавляет новую вакансию в базу данных.
        /// </summary>
        /// <param_name="vacancy">Объект вакансии для добавления.</param>
        public async Task AddVacancyAsync(Vacancy vacancy)
        {
            _context.Vacancies.Add(vacancy);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
        }

        /// <summary>
        /// Обновляет существующую вакансию в базе данных.
        /// Использует новый контекст и подгружает запись заново,
        /// чтобы избежать конфликта отслеживания сущностей.
        /// </summary>
        /// <param_name="vacancy">Объект вакансии с новыми данными.</param>
        public async Task UpdateVacancyAsync(Vacancy vacancy)
        {
            using var context = new HrDbContext();
            var existing = await context.Vacancies.FindAsync(vacancy.Id);
            if (existing != null)
            {
                existing.Title = vacancy.Title;
                existing.Description = vacancy.Description;
                existing.Requirements = vacancy.Requirements;
                existing.Salary = vacancy.Salary;
                existing.Status = vacancy.Status;
                existing.PostedDate = vacancy.PostedDate;
                existing.LastUpdated = vacancy.LastUpdated;
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Удаляет вакансию по идентификатору.
        /// Перед удалением стоит проверить, что на вакансию не назначены собеседования
        /// (см. HasInterviewsAsync).
        /// </summary>
        /// <param_name="id">Идентификатор вакансии.</param>
        public async Task DeleteVacancyAsync(int id)
        {
            var vacancy = await _context.Vacancies.FindAsync(id);
            if (vacancy != null)
            {
                _context.Vacancies.Remove(vacancy);
                await _context.SaveChangesAsync();
                _context.ChangeTracker.Clear();
            }
        }

        /// <summary>
        /// Проверяет, существует ли вакансия с таким названием.
        /// Используется для валидации уникальности названия.
        /// Сравнение регистронезависимое.
        /// </summary>
        /// <param_name="title">Название вакансии для проверки.</param>
        /// <param_name="excludeId">ID для исключения (при редактировании).</param>
        /// <returns>True, если вакансия с таким названием уже существует.</returns>
        public async Task<bool> IsTitleExistsAsync(string title, int? excludeId = null)
        {
            var query = _context.Vacancies.Where(v => v.Title.ToLower() == title.ToLower());

            if (excludeId.HasValue)
                query = query.Where(v => v.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        /// <summary>
        /// Проверяет, назначены ли на вакансию собеседования.
        /// Используется для запрета удаления вакансии, если на неё есть собеседования.
        /// </summary>
        /// <param_name="vacancyId">Идентификатор вакансии.</param>
        /// <returns>True, если на вакансию назначено хотя бы одно собеседование.</returns>
        public async Task<bool> HasInterviewsAsync(int vacancyId)
        {
            return await _context.Interviews.AnyAsync(i => i.VacancyId == vacancyId);
        }
    }
}

