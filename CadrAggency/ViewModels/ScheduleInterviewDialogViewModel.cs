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
    /// ViewModel диалога назначения собеседования.
    /// Загружает список активных соискателей и открытых вакансий,
    /// принимает дату, время, формат и комментарий.
    /// </summary>
    public class ScheduleInterviewDialogViewModel : INotifyPropertyChanged
    {
        private readonly InterviewService _service;

        /// <summary>
        /// Конструктор. Задаёт дату по умолчанию (завтра) и время (10:00).
        /// </summary>
        /// <param name="service">Сервис для работы с собеседованиями.</param>
        public ScheduleInterviewDialogViewModel(InterviewService service)
        {
            _service = service;
            ScheduledDate = DateTimeOffset.Now.Date.AddDays(1);
            ScheduledTime = new TimeSpan(10, 0, 0);

            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(this, EventArgs.Empty));

            _ = LoadDataAsync();
        }

        private List<Candidate> _candidates = new();
        public List<Candidate> Candidates
        {
            get => _candidates;
            set { _candidates = value; OnPropertyChanged(); }
        }

        private List<Vacancy> _vacancies = new();
        public List<Vacancy> Vacancies
        {
            get => _vacancies;
            set { _vacancies = value; OnPropertyChanged(); }
        }

        /// <summary>Список доступных форматов собеседования.</summary>
        public List<InterviewFormat> Formats =>
            Enum.GetValues(typeof(InterviewFormat)).Cast<InterviewFormat>().ToList();

        private Candidate? _selectedCandidate;
        public Candidate? SelectedCandidate
        {
            get => _selectedCandidate;
            set { _selectedCandidate = value; OnPropertyChanged(); }
        }

        private Vacancy? _selectedVacancy;
        public Vacancy? SelectedVacancy
        {
            get => _selectedVacancy;
            set { _selectedVacancy = value; OnPropertyChanged(); }
        }

        private DateTimeOffset? _scheduledDate;
        public DateTimeOffset? ScheduledDate
        {
            get => _scheduledDate;
            set { _scheduledDate = value; OnPropertyChanged(); }
        }

        private TimeSpan? _scheduledTime;
        public TimeSpan? ScheduledTime
        {
            get => _scheduledTime;
            set { _scheduledTime = value; OnPropertyChanged(); }
        }

        private InterviewFormat _format = InterviewFormat.Offline;
        public InterviewFormat Format
        {
            get => _format;
            set { _format = value; OnPropertyChanged(); }
        }

        private string _comment = string.Empty;
        public string Comment
        {
            get => _comment;
            set { _comment = value; OnPropertyChanged(); }
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        /// <summary>Событие запроса на закрытие диалогового окна.</summary>
        public event EventHandler? RequestClose;

        /// <summary>Событие успешного создания собеседования.</summary>
        public event EventHandler<Interview>? InterviewScheduled;

        /// <summary>
        /// Загружает активных соискателей и открытые вакансии для ComboBox.
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                var candidates = await _service.GetActiveCandidatesAsync();
                var vacancies = await _service.GetOpenVacanciesAsync();

                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Candidates = candidates;
                    Vacancies = vacancies;
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки списков: {ex.Message}";
            }
        }

        /// <summary>
        /// Проверяет, что все обязательные поля заполнены.
        /// </summary>
        private bool Validate()
        {
            if (SelectedCandidate == null)
            {
                ErrorMessage = "Выберите соискателя.";
                return false;
            }
            if (SelectedVacancy == null)
            {
                ErrorMessage = "Выберите вакансию.";
                return false;
            }
            if (ScheduledDate == null)
            {
                ErrorMessage = "Укажите дату собеседования.";
                return false;
            }
            if (ScheduledTime == null)
            {
                ErrorMessage = "Укажите время собеседования.";
                return false;
            }

            ErrorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Объединяет дату и время в один DateTime и передаёт собеседование
        /// через событие InterviewScheduled.
        /// </summary>
        private async Task SaveAsync()
        {
            if (!Validate()) return;

            var dateTime = ScheduledDate!.Value.Date + ScheduledTime!.Value;

            var interview = new Interview
            {
                CandidateId = SelectedCandidate!.Id,
                VacancyId = SelectedVacancy!.Id,
                ScheduledDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
                Format = Format,
                Comment = string.IsNullOrWhiteSpace(Comment) ? null : Comment,
                Result = null
            };

            InterviewScheduled?.Invoke(this, interview);
            await Task.CompletedTask;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

