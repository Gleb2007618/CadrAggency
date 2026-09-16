using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CadrAggency.Models;
using CadrAggency.Models.Enums;
using CadrAggency.Services;
using CadrAggency.Views;

namespace CadrAggency.ViewModels
{
    /// <summary>
    /// ViewModel вкладки «Собеседования».
    /// Управляет таблицей собеседований, фильтрами,
    /// назначением и завершением собеседований.
    /// </summary>
    public class InterviewTabViewModel : INotifyPropertyChanged
    {
        private readonly InterviewService _service;
        private readonly CandidateService _candidateService;

        /// <summary>
        /// Конструктор. Запускает загрузку собеседований и списков для фильтров.
        /// </summary>
        /// <param name="service">Сервис для работы с собеседованиями.</param>
        /// <param name="candidateService">Сервис для работы с кандидатами.</param>
        public InterviewTabViewModel(InterviewService service, CandidateService candidateService)
        {
            _service = service;
            _candidateService = candidateService;
            _ = LoadInterviewsAsync();
            _ = LoadFilterListsAsync();
        }

        private ObservableCollection<Interview> _interviews = new();
        public ObservableCollection<Interview> Interviews
        {
            get => _interviews;
            set { _interviews = value; OnPropertyChanged(); }
        }

        private List<Candidate> _candidatesForFilter = new();
        public List<Candidate> CandidatesForFilter
        {
            get => _candidatesForFilter;
            set { _candidatesForFilter = value; OnPropertyChanged(); }
        }

        private List<Vacancy> _vacanciesForFilter = new();
        public List<Vacancy> VacanciesForFilter
        {
            get => _vacanciesForFilter;
            set { _vacanciesForFilter = value; OnPropertyChanged(); }
        }

        /// <summary>Список возможных результатов собеседования для ComboBox.</summary>
        public List<InterviewResult> Results =>
            Enum.GetValues(typeof(InterviewResult)).Cast<InterviewResult>().ToList();

        private Interview? _selectedInterview;
        public Interview? SelectedInterview
        {
            get => _selectedInterview;
            set
            {
                _selectedInterview = value;
                OnPropertyChanged();
                _completeCommand?.RaiseCanExecuteChanged();
                _deleteCommand?.RaiseCanExecuteChanged();
            }
        }

        private Candidate? _filterCandidate;
        public Candidate? FilterCandidate
        {
            get => _filterCandidate;
            set { _filterCandidate = value; OnPropertyChanged(); }
        }

        private Vacancy? _filterVacancy;
        public Vacancy? FilterVacancy
        {
            get => _filterVacancy;
            set { _filterVacancy = value; OnPropertyChanged(); }
        }

        private InterviewResult? _filterResult;
        public InterviewResult? FilterResult
        {
            get => _filterResult;
            set { _filterResult = value; OnPropertyChanged(); }
        }

        public ICommand LoadCommand => new RelayCommand(async _ => await LoadInterviewsAsync());
        public ICommand ScheduleCommand => new RelayCommand(async _ => await ScheduleInterviewAsync());

        // Кнопка «Завершить» активна только для собеседований с прошедшей датой
        private RelayCommand? _completeCommand;
        public ICommand CompleteCommand => _completeCommand ??= new RelayCommand(
            async _ => await CompleteInterviewAsync(),
            _ => SelectedInterview != null && SelectedInterview.ScheduledDateTime < DateTime.UtcNow);

        private RelayCommand? _deleteCommand;
        public ICommand DeleteCommand => _deleteCommand ??= new RelayCommand(
            async _ => await DeleteInterviewAsync(),
            _ => SelectedInterview != null);

        public ICommand ApplyFiltersCommand => new RelayCommand(async _ => await LoadInterviewsAsync());

        public ICommand ResetFiltersCommand => new RelayCommand(async _ =>
        {
            FilterCandidate = null;
            FilterVacancy = null;
            FilterResult = null;
            await LoadInterviewsAsync();
        });

        /// <summary>
        /// Загружает список собеседований с учётом фильтров.
        /// </summary>
        private async Task LoadInterviewsAsync()
        {
            try
            {
                var list = await _service.GetFilteredInterviewsAsync(
                    FilterCandidate?.Id,
                    FilterVacancy?.Id,
                    FilterResult);

                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Interviews.Clear();
                    foreach (var i in list)
                        Interviews.Add(i);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки собеседований: {ex.Message}");
            }
        }

        /// <summary>
        /// Загружает списки кандидатов и вакансий для фильтров.
        /// </summary>
        private async Task LoadFilterListsAsync()
        {
            try
            {
                var candidates = await _service.GetAllCandidatesForFilterAsync();
                var vacancies = await _service.GetAllVacanciesForFilterAsync();

                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    CandidatesForFilter = candidates;
                    VacanciesForFilter = vacancies;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки списков фильтров: {ex.Message}");
            }
        }

        /// <summary>
        /// Открывает диалог назначения нового собеседования.
        /// </summary>
        private async Task ScheduleInterviewAsync()
        {
            var mainWindow = (Application.Current?.ApplicationLifetime
                as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow == null) return;

            var dialog = new ScheduleInterviewDialog();
            var vm = new ScheduleInterviewDialogViewModel(_service);
            dialog.DataContext = vm;

            vm.RequestClose += (s, e) => dialog.Close();
            vm.InterviewScheduled += async (s, interview) =>
            {
                try
                {
                    await _service.AddInterviewAsync(interview);
                    await LoadInterviewsAsync();
                }
                catch (Exception ex)
                {
                    await ShowInfoAsync($"Ошибка сохранения собеседования: {ex.Message}");
                }
            };

            await dialog.ShowDialog(mainWindow);
        }

        /// <summary>
        /// Открывает диалог завершения собеседования, сохраняет результат
        /// и автоматически обновляет статус соискателя и вакансии.
        /// </summary>
        private async Task CompleteInterviewAsync()
        {
            if (SelectedInterview == null) return;

            var mainWindow = (Application.Current?.ApplicationLifetime
                as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow == null) return;

            var dialog = new CompleteInterviewDialog();
            var vm = new CompleteInterviewDialogViewModel(SelectedInterview);
            dialog.DataContext = vm;

            vm.RequestClose += (s, e) => dialog.Close();
            vm.InterviewCompleted += async (s, interview) =>
            {
                try
                {
                    using (var context = new CadrAggency.Data.HrDbContext())
                    {
                        // Сохраняем результат собеседования
                        var existing = await context.Interviews.FindAsync(interview.Id);
                        if (existing != null)
                        {
                            existing.Result = interview.Result;
                            existing.Comment = interview.Comment;
                            await context.SaveChangesAsync();
                        }

                        // При успехе или следующем этапе меняем статус соискателя
                        if (interview.Result == InterviewResult.Successful ||
                            interview.Result == InterviewResult.NextStage)
                        {
                            var candidate = await context.Candidates.FindAsync(interview.CandidateId);
                            if (candidate != null)
                            {
                                candidate.Status = interview.Result == InterviewResult.Successful
                                    ? CandidateStatus.Employed
                                    : CandidateStatus.Pending;
                                candidate.LastUpdated = DateTime.UtcNow;
                                await context.SaveChangesAsync();
                            }
                        }

                        // Если собеседование успешно — вакансия закрывается
                        if (interview.Result == InterviewResult.Successful)
                        {
                            var vacancy = await context.Vacancies.FindAsync(interview.VacancyId);
                            if (vacancy != null)
                            {
                                vacancy.Status = VacancyStatus.Closed;
                                vacancy.LastUpdated = DateTime.UtcNow;
                                await context.SaveChangesAsync();
                            }
                        }
                    }

                    await LoadInterviewsAsync();

                    if (mainWindow.DataContext is MainWindowViewModel mainVm)
                    {
                        await mainVm.RefreshAfterInterviewAsync();
                    }
                }
                catch (Exception ex)
                {
                    await ShowInfoAsync($"Ошибка завершения собеседования: {ex.Message}");
                }
            };

            await dialog.ShowDialog(mainWindow);
        }

        /// <summary>
        /// Удаляет выбранное собеседование с подтверждением.
        /// </summary>
        private async Task DeleteInterviewAsync()
        {
            if (SelectedInterview == null) return;

            var mainWindow = (Application.Current?.ApplicationLifetime
                as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow == null) return;

            var dialog = new ConfirmDialog();
            var vm = new ConfirmDialogViewModel(
                $"Удалить собеседование {SelectedInterview.Candidate?.FullName} — {SelectedInterview.Vacancy?.Title}?");
            dialog.DataContext = vm;
            vm.RequestClose += (s, e) => dialog.Close();

            await dialog.ShowDialog(mainWindow);
            if (!vm.Result) return;

            try
            {
                await _service.DeleteInterviewAsync(SelectedInterview.Id);
                await LoadInterviewsAsync();
            }
            catch (Exception ex)
            {
                await ShowInfoAsync($"Ошибка удаления собеседования: {ex.Message}");
            }
        }

        /// <summary>
        /// Показывает информационное окно с сообщением.
        /// </summary>
        /// <param name="message">Текст для отображения.</param>
        private async Task ShowInfoAsync(string message)
        {
            var mainWindow = (Application.Current?.ApplicationLifetime
                as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow == null) return;

            var dialog = new ConfirmDialog();
            var vm = new ConfirmDialogViewModel(message);
            dialog.DataContext = vm;
            vm.RequestClose += (s, e) => dialog.Close();
            await dialog.ShowDialog(mainWindow);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

