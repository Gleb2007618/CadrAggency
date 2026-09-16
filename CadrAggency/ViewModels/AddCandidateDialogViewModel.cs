using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
    /// ViewModel диалогового окна добавления нового соискателя.
    /// Отвечает за валидацию полей и создание объекта Candidate.
    /// </summary>
    public class AddCandidateDialogViewModel : INotifyPropertyChanged
    {
        private readonly CandidateService _service;

        /// <summary>
        /// Конструктор. Создаёт команды сохранения и отмены.
        /// </summary>
        /// <param name="service">Сервис для работы с кандидатами.</param>
        public AddCandidateDialogViewModel(CandidateService service)
        {
            _service = service;
            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(this, EventArgs.Empty));
        }

        private string _fullName = string.Empty;
        public string FullName { get => _fullName; set { _fullName = value; OnPropertyChanged(); } }

        private string _telephone = string.Empty;
        public string Telephone { get => _telephone; set { _telephone = value; OnPropertyChanged(); } }

        private string _email = string.Empty;
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }

        private DateTimeOffset? _birthDate = DateTimeOffset.Now;
        public DateTimeOffset? BirthDate { get => _birthDate; set { _birthDate = value; OnPropertyChanged(); } }

        private EducationLevel _education = EducationLevel.Higher;
        public EducationLevel Education { get => _education; set { _education = value; OnPropertyChanged(); } }

        private string _skills = string.Empty;
        public string Skills { get => _skills; set { _skills = value; OnPropertyChanged(); } }

        private int? _experienceYears = 0;
        public int? ExperienceYears { get => _experienceYears; set { _experienceYears = value; OnPropertyChanged(); } }

        private decimal? _expectedSalary = 0;
        public decimal? ExpectedSalary { get => _expectedSalary; set { _expectedSalary = value; OnPropertyChanged(); } }

        private CandidateStatus _status = CandidateStatus.Active;
        public CandidateStatus Status { get => _status; set { _status = value; OnPropertyChanged(); } }

        private string _resumeText = string.Empty;
        public string ResumeText { get => _resumeText; set { _resumeText = value; OnPropertyChanged(); } }

        private string _resumeFilePath = string.Empty;
        public string ResumeFilePath { get => _resumeFilePath; set { _resumeFilePath = value; OnPropertyChanged(); } }

        private string _errorMessage = string.Empty;
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }

        /// <summary>Список уровней образования для ComboBox.</summary>
        public List<EducationLevel> EducationLevels =>
            Enum.GetValues(typeof(EducationLevel)).Cast<EducationLevel>().ToList();

        /// <summary>Список статусов кандидата для ComboBox.</summary>
        public List<CandidateStatus> Statuses =>
            Enum.GetValues(typeof(CandidateStatus)).Cast<CandidateStatus>().ToList();

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AttachFileCommand => new RelayCommand(_ => AttachFile());

        /// <summary>Событие запроса на закрытие диалогового окна.</summary>
        public event EventHandler? RequestClose;

        /// <summary>Событие успешного создания нового кандидата.</summary>
        public event EventHandler<Candidate>? CandidateAdded;

        /// <summary>
        /// Проверяет корректность введённых данных.
        /// Возвращает false, если есть ошибки, и заполняет ErrorMessage.
        /// </summary>
        private async Task<bool> ValidateAsync()
        {
            if (string.IsNullOrWhiteSpace(FullName))
            {
                ErrorMessage = "Полное имя обязательно для заполнения.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(Telephone))
            {
                ErrorMessage = "Телефон обязателен для заполнения.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Email обязателен для заполнения.";
                return false;
            }
            if ((ExperienceYears ?? 0) < 0)
            {
                ErrorMessage = "Опыт работы не может быть отрицательным.";
                return false;
            }
            if ((ExpectedSalary ?? 0) < 0)
            {
                ErrorMessage = "Зарплата не может быть отрицательной.";
                return false;
            }
            if (await _service.IsFullNameExistsAsync(FullName))
            {
                ErrorMessage = "Кандидат с таким именем уже существует.";
                return false;
            }

            ErrorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Валидирует данные, формирует объект Candidate
        /// и передаёт его через событие CandidateAdded.
        /// </summary>
        private async Task SaveAsync()
        {
            if (!await ValidateAsync()) return;

            var candidate = new Candidate
            {
                FullName = FullName,
                Telephone = Telephone,
                Email = Email,
                BirthDate = DateTime.SpecifyKind(BirthDate?.DateTime ?? DateTime.Today, DateTimeKind.Utc),
                Education = Education,
                Skills = Skills,
                ExperienceYears = ExperienceYears ?? 0,
                ExpectedSalary = ExpectedSalary ?? 0,
                Status = Status,
                ResumeText = ResumeText,
                ResumeFilePath = string.IsNullOrWhiteSpace(ResumeFilePath) ? null : ResumeFilePath,
                LastUpdated = DateTime.UtcNow
            };

            CandidateAdded?.Invoke(this, candidate);
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Открывает диалог выбора файла и сохраняет путь к резюме.
        /// </summary>
        private async void AttachFile()
        {
            var options = new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                Title = "Выберите файл резюме",
                AllowMultiple = false
            };

            var topLevel = Avalonia.Application.Current?.ApplicationLifetime is
                Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow : null;

            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
            if (files.Count > 0)
            {
                ResumeFilePath = files[0].Path.LocalPath;
                OnPropertyChanged(nameof(ResumeFilePath));
                ErrorMessage = $"Файл прикреплён: {System.IO.Path.GetFileName(ResumeFilePath)}";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


