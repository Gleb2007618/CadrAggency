using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CadrAggency.Models;
using CadrAggency.Models.Enums;
using CadrAggency.Services;

namespace CadrAggency.ViewModels
{
    /// <summary>
    /// ViewModel диалога вакансии.
    /// Работает в двух режимах: добавление (editingVacancy == null)
    /// и редактирование (передан существующий объект).
    /// </summary>
    public class VacancyDialogViewModel : INotifyPropertyChanged
    {
        private readonly VacancyService _service;
        private readonly Vacancy? _editingVacancy;

        /// <summary>
        /// Конструктор. Если передан объект — переходит в режим редактирования.
        /// </summary>
        /// <param name="service">Сервис для работы с вакансиями.</param>
        /// <param name="editingVacancy">Редактируемая вакансия (null для добавления).</param>
        public VacancyDialogViewModel(VacancyService service, Vacancy? editingVacancy = null)
        {
            _service = service;
            _editingVacancy = editingVacancy;

            if (editingVacancy != null)
            {
                Title = editingVacancy.Title;
                Description = editingVacancy.Description;
                Requirements = editingVacancy.Requirements;
                Salary = editingVacancy.Salary;
                Status = editingVacancy.Status;
                PostedDate = new DateTimeOffset(editingVacancy.PostedDate, TimeSpan.Zero);
            }
            else
            {
                PostedDate = DateTimeOffset.Now;
                Status = VacancyStatus.Open;
            }

            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(this, EventArgs.Empty));
        }

        private string _title = string.Empty;
        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }

        private string _description = string.Empty;
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }

        private string _requirements = string.Empty;
        public string Requirements { get => _requirements; set { _requirements = value; OnPropertyChanged(); } }

        private decimal? _salary = 0;
        public decimal? Salary { get => _salary; set { _salary = value; OnPropertyChanged(); } }

        private VacancyStatus _status = VacancyStatus.Open;
        public VacancyStatus Status { get => _status; set { _status = value; OnPropertyChanged(); } }

        private DateTimeOffset? _postedDate = DateTimeOffset.Now;
        public DateTimeOffset? PostedDate { get => _postedDate; set { _postedDate = value; OnPropertyChanged(); } }

        private string _errorMessage = string.Empty;
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }

        /// <summary>Список статусов вакансии для ComboBox.</summary>
        public List<VacancyStatus> Statuses =>
            Enum.GetValues(typeof(VacancyStatus)).Cast<VacancyStatus>().ToList();

        /// <summary>Заголовок окна в зависимости от режима.</summary>
        public string WindowTitle => _editingVacancy == null ? "Добавление вакансии" : "Редактирование вакансии";

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        /// <summary>Событие запроса на закрытие диалогового окна.</summary>
        public event EventHandler? RequestClose;

        /// <summary>Событие сохранения вакансии (добавления или обновления).</summary>
        public event EventHandler<Vacancy>? VacancySaved;

        /// <summary>
        /// Проверяет корректность введённых данных.
        /// Учитывает уникальность названия (кроме редактируемой вакансии).
        /// </summary>
        private async Task<bool> ValidateAsync()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                ErrorMessage = "Название вакансии обязательно для заполнения.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(Description))
            {
                ErrorMessage = "Описание обязательно для заполнения.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(Requirements))
            {
                ErrorMessage = "Требования обязательны для заполнения.";
                return false;
            }
            if ((Salary ?? 0) < 0)
            {
                ErrorMessage = "Зарплата не может быть отрицательной.";
                return false;
            }

            int? excludeId = _editingVacancy?.Id;
            if (await _service.IsTitleExistsAsync(Title, excludeId))
            {
                ErrorMessage = "Вакансия с таким названием уже существует.";
                return false;
            }

            ErrorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Формирует объект Vacancy и передаёт его через событие VacancySaved.
        /// </summary>
        private async Task SaveAsync()
        {
            if (!await ValidateAsync()) return;

            Vacancy vacancy;
            if (_editingVacancy == null)
            {
                vacancy = new Vacancy
                {
                    Title = Title,
                    Description = Description,
                    Requirements = Requirements,
                    Salary = Salary ?? 0,
                    Status = Status,
                    PostedDate = DateTime.SpecifyKind(PostedDate?.DateTime ?? DateTime.Today, DateTimeKind.Utc),
                    LastUpdated = DateTime.UtcNow
                };
            }
            else
            {
                vacancy = _editingVacancy;
                vacancy.Title = Title;
                vacancy.Description = Description;
                vacancy.Requirements = Requirements;
                vacancy.Salary = Salary ?? 0;
                vacancy.Status = Status;
                vacancy.PostedDate = DateTime.SpecifyKind(PostedDate?.DateTime ?? DateTime.Today, DateTimeKind.Utc);
                vacancy.LastUpdated = DateTime.UtcNow;
            }

            VacancySaved?.Invoke(this, vacancy);
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


