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
    /// ViewModel диалогового окна редактирования соискателя.
    /// Поля предзаполняются данными выбранного кандидата.
    /// </summary>
    public class EditCandidateDialogViewModel : INotifyPropertyChanged
    {
        private readonly CandidateService _service;
        private readonly Candidate _candidate;

        /// <summary>
        /// Конструктор. Заполняет поля данными переданного кандидата.
        /// </summary>
        /// <param name="service">Сервис для работы с кандидатами.</param>
        /// <param name="candidate">Редактируемый кандидат.</param>
        public EditCandidateDialogViewModel(CandidateService service, Candidate candidate)
        {
            _service = service;
            _candidate = candidate;

            FullName = candidate.FullName;
            Telephone = candidate.Telephone;
            Email = candidate.Email;
            BirthDate = new DateTimeOffset(candidate.BirthDate, TimeSpan.Zero);
            Education = candidate.Education;
            Skills = candidate.Skills;
            ExperienceYears = candidate.ExperienceYears;
            ExpectedSalary = candidate.ExpectedSalary;
            Status = candidate.Status;
            ResumeText = candidate.ResumeText;
            ResumeFilePath = candidate.ResumeFilePath ?? string.Empty;

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

        /// <summary>Событие успешного обновления данных кандидата.</summary>
        public event EventHandler<Candidate>? CandidateUpdated;

        /// <summary>
        /// Проверяет корректность введённых данных.
        /// Учитывает, что имя может совпадать с самим редактируемым кандидатом.
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
            if (await _service.IsFullNameExistsAsync(FullName, _candidate.Id))
            {
                ErrorMessage = "Кандидат с таким именем уже существует.";
                return false;
            }

            ErrorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Применяет введённые данные к объекту кандидата
        /// и передаёт его через событие CandidateUpdated.
        /// </summary>
        private async Task SaveAsync()
        {
            if (!await ValidateAsync()) return;

            _candidate.FullName = FullName;
            _candidate.Telephone = Telephone;
            _candidate.Email = Email;
            _candidate.BirthDate = DateTime.SpecifyKind(BirthDate?.DateTime ?? DateTime.Today, DateTimeKind.Utc);
            _candidate.Education = Education;
            _candidate.Skills = Skills;
            _candidate.ExperienceYears = ExperienceYears ?? 0;
            _candidate.ExpectedSalary = ExpectedSalary ?? 0;
            _candidate.Status = Status;
            _candidate.ResumeText = ResumeText;
            _candidate.ResumeFilePath = string.IsNullOrWhiteSpace(ResumeFilePath) ? null : ResumeFilePath;
            _candidate.LastUpdated = DateTime.UtcNow;

            CandidateUpdated?.Invoke(this, _candidate);
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


