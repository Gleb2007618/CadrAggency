using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CadrAggency.Data;
using CadrAggency.Models;
using CadrAggency.Models.Enums;
using CadrAggency.Services;
using CadrAggency.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CadrAggency.ViewModels
{
    /// <summary>
    /// Главная ViewModel приложения.
    /// Управляет вкладкой кандидатов и хранит ссылки на ViewModel
    /// других вкладок (вакансии, собеседования).
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly CandidateService _service;
        private readonly CsvService _csvService = new();

        /// <summary>ViewModel вкладки «Вакансии».</summary>
        public VacancyTabViewModel VacancyTab { get; }

        /// <summary>ViewModel вкладки «Собеседования».</summary>
        public InterviewTabViewModel InterviewTab { get; }

        /// <summary>
        /// Конструктор главной ViewModel.
        /// Создаёт сервисы и ViewModel вкладок, запускает загрузку кандидатов.
        /// </summary>
        /// <param name="service">Сервис для работы с кандидатами.</param>
        public MainWindowViewModel(CandidateService service)
        {
            _service = service;

            var vacancyService = new VacancyService(new HrDbContext());
            VacancyTab = new VacancyTabViewModel(vacancyService);

            var interviewService = new InterviewService(new HrDbContext());
            InterviewTab = new InterviewTabViewModel(interviewService, service);

            _ = LoadCandidatesAsync();
        }

        private ObservableCollection<Candidate> _candidates = new();
        public ObservableCollection<Candidate> Candidates
        {
            get => _candidates;
            set { _candidates = value; OnPropertyChanged(); }
        }

        private Candidate? _selectedCandidate;
        public Candidate? SelectedCandidate
        {
            get => _selectedCandidate;
            set
            {
                _selectedCandidate = value;
                OnPropertyChanged();
                _editCandidateCommand?.RaiseCanExecuteChanged();
                _deleteCandidateCommand?.RaiseCanExecuteChanged();
            }
        }

        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set { _searchQuery = value; OnPropertyChanged(); }
        }

        private CandidateStatus? _selectedStatus;
        public CandidateStatus? SelectedStatus
        {
            get => _selectedStatus;
            set { _selectedStatus = value; OnPropertyChanged(); }
        }

        private EducationLevel? _selectedEducation;
        public EducationLevel? SelectedEducation
        {
            get => _selectedEducation;
            set { _selectedEducation = value; OnPropertyChanged(); }
        }

        private decimal? _minSalary;
        public decimal? MinSalary
        {
            get => _minSalary;
            set { _minSalary = value; OnPropertyChanged(); }
        }

        private decimal? _maxSalary;
        public decimal? MaxSalary
        {
            get => _maxSalary;
            set { _maxSalary = value; OnPropertyChanged(); }
        }

        private int _totalCandidates;
        public int TotalCandidates
        {
            get => _totalCandidates;
            set { _totalCandidates = value; OnPropertyChanged(); }
        }

        private int _activeVacancies;
        public int ActiveVacancies
        {
            get => _activeVacancies;
            set { _activeVacancies = value; OnPropertyChanged(); }
        }

        private int _weeklyInterviews;
        public int WeeklyInterviews
        {
            get => _weeklyInterviews;
            set { _weeklyInterviews = value; OnPropertyChanged(); }
        }

        private int _monthlySuccessful;
        public int MonthlySuccessful
        {
            get => _monthlySuccessful;
            set { _monthlySuccessful = value; OnPropertyChanged(); }
        }

        /// <summary>Список возможных статусов кандидата для ComboBox.</summary>
        public List<CandidateStatus> Statuses => Enum.GetValues(typeof(CandidateStatus)).Cast<CandidateStatus>().ToList();

        /// <summary>Список уровней образования для ComboBox.</summary>
        public List<EducationLevel> EducationLevels => Enum.GetValues(typeof(EducationLevel)).Cast<EducationLevel>().ToList();

        public ICommand LoadDataCommand => new RelayCommand(async _ => await LoadCandidatesAsync());
        public ICommand AddCandidateCommand => new RelayCommand(_ => AddCandidate());
        public ICommand ExportCandidatesCommand => new RelayCommand(async _ => await ExportCandidatesAsync());
        public ICommand ImportCandidatesCommand => new RelayCommand(async _ => await ImportCandidatesAsync());

        private RelayCommand? _editCandidateCommand;
        public ICommand EditCandidateCommand => _editCandidateCommand ??= new RelayCommand(_ => EditCandidate(), _ => SelectedCandidate != null);

        private RelayCommand? _deleteCandidateCommand;
        public ICommand DeleteCandidateCommand => _deleteCandidateCommand ??= new RelayCommand(async _ => await DeleteCandidateAsync(), _ => SelectedCandidate != null);

        public ICommand SearchCommand => new RelayCommand(async _ => await LoadCandidatesAsync());
        public ICommand ResetFiltersCommand => new RelayCommand(async _ => await ResetFiltersAsync());

        /// <summary>
        /// Загружает список кандидатов с учётом текущих фильтров
        /// и обновляет карточки аналитики.
        /// </summary>
        private async Task LoadCandidatesAsync()
        {
            await RunWithWaitCursorAsync(async () =>
            {
                try
                {
                    var list = await _service.GetFilteredCandidatesAsync(
                        SearchQuery,
                        SelectedStatus,
                        SelectedEducation,
                        MinSalary,
                        MaxSalary
                    );
                    Candidates.Clear();
                    foreach (var c in list)
                        Candidates.Add(c);

                    await UpdateAnalyticsAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка загрузки: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Сбрасывает все фильтры и перезагружает таблицу кандидатов.
        /// </summary>
        private async Task ResetFiltersAsync()
        {
            SearchQuery = string.Empty;
            SelectedStatus = null;
            SelectedEducation = null;
            MinSalary = null;
            MaxSalary = null;
            await LoadCandidatesAsync();
        }

        /// <summary>
        /// Обновляет значения четырёх карточек аналитики.
        /// </summary>
        private async Task UpdateAnalyticsAsync()
        {
            try
            {
                TotalCandidates = await _service.GetTotalCandidatesCountAsync();
                ActiveVacancies = await _service.GetActiveVacanciesCountAsync();
                WeeklyInterviews = await _service.GetWeeklyInterviewsCountAsync();
                MonthlySuccessful = await _service.GetMonthlySuccessfulInterviewsCountAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления аналитики: {ex.Message}");
            }
        }

        /// <summary>
        /// Открывает диалог добавления нового кандидата.
        /// </summary>
        private async void AddCandidate()
        {
            try
            {
                var mainWindow = (Application.Current?.ApplicationLifetime
                    as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

                if (mainWindow == null) return;

                var dialog = new AddCandidateDialog();
                var viewModel = new AddCandidateDialogViewModel(_service);
                dialog.DataContext = viewModel;

                viewModel.RequestClose += (s, e) => dialog.Close();

                viewModel.CandidateAdded += async (s, candidate) =>
                {
                    try
                    {
                        await _service.AddCandidateAsync(candidate);
                        await LoadCandidatesAsync();
                    }
                    catch (Exception ex)
                    {
                        await ShowInfoAsync($"Ошибка сохранения: {ex.Message}");
                    }
                };

                await dialog.ShowDialog(mainWindow);
            }
            catch (Exception ex)
            {
                await ShowInfoAsync($"Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Открывает диалог редактирования выбранного кандидата.
        /// </summary>
        private async void EditCandidate()
        {
            if (SelectedCandidate == null) return;

            var mainWindow = (Application.Current?.ApplicationLifetime
                as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow == null) return;

            var dialog = new EditCandidateDialog();
            var viewModel = new EditCandidateDialogViewModel(_service, SelectedCandidate);
            dialog.DataContext = viewModel;

            viewModel.RequestClose += (s, e) => dialog.Close();
            viewModel.CandidateUpdated += async (s, candidate) =>
            {
                try
                {
                    await _service.UpdateCandidateAsync(candidate);
                    await LoadCandidatesAsync();
                }
                catch (Exception ex)
                {
                    await ShowInfoAsync($"Ошибка редактирования: {ex.Message}");
                }
            };

            await dialog.ShowDialog(mainWindow);
        }

        /// <summary>
        /// Удаляет выбранного кандидата с подтверждением.
        /// </summary>
        private async Task DeleteCandidateAsync()
        {
            if (SelectedCandidate == null) return;

            var mainWindow = (Application.Current?.ApplicationLifetime
                as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow == null) return;

            var dialog = new ConfirmDialog();
            var viewModel = new ConfirmDialogViewModel(
                $"Вы уверены, что хотите удалить кандидата «{SelectedCandidate.FullName}»?");
            dialog.DataContext = viewModel;
            viewModel.RequestClose += (s, e) => dialog.Close();

            await dialog.ShowDialog(mainWindow);

            if (!viewModel.Result) return;

            try
            {
                await _service.DeleteCandidateAsync(SelectedCandidate.Id);
                await LoadCandidatesAsync();
            }
            catch (Exception ex)
            {
                await ShowInfoAsync($"Ошибка удаления: {ex.Message}");
            }
        }

        /// <summary>
        /// Экспортирует текущий отфильтрованный список кандидатов в CSV-файл.
        /// </summary>
        private async Task ExportCandidatesAsync()
        {
            try
            {
                var mainWindow = (Application.Current?.ApplicationLifetime
                    as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
                if (mainWindow == null) return;

                var file = await mainWindow.StorageProvider.SaveFilePickerAsync(
                    new Avalonia.Platform.Storage.FilePickerSaveOptions
                    {
                        Title = "Сохранить CSV",
                        SuggestedFileName = "candidates.csv",
                        DefaultExtension = "csv",
                        FileTypeChoices = new[]
                        {
                            new Avalonia.Platform.Storage.FilePickerFileType("CSV")
                            {
                                Patterns = new[] { "*.csv" }
                            }
                        }
                    });

                if (file == null) return;

                var path = file.Path.LocalPath;

                var list = await _service.GetFilteredCandidatesAsync(
                    SearchQuery,
                    SelectedStatus,
                    SelectedEducation,
                    MinSalary,
                    MaxSalary);

                _csvService.ExportCandidates(path, list);
                await ShowInfoAsync($"Экспортировано {list.Count} кандидатов в файл:\n{path}");
            }
            catch (Exception ex)
            {
                await ShowInfoAsync($"Ошибка экспорта: {ex.Message}");
            }
        }

        /// <summary>
        /// Импортирует кандидатов из CSV-файла, пропуская дубликаты по имени и телефону.
        /// </summary>
        private async Task ImportCandidatesAsync()
        {
            await RunWithWaitCursorAsync(async () =>
            {
                try
                {
                    var mainWindow = (Application.Current?.ApplicationLifetime
                        as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
                    if (mainWindow == null) return;

                    var files = await mainWindow.StorageProvider.OpenFilePickerAsync(
                        new Avalonia.Platform.Storage.FilePickerOpenOptions
                        {
                            Title = "Выберите CSV-файл",
                            AllowMultiple = false,
                            FileTypeFilter = new[]
                            {
                                new Avalonia.Platform.Storage.FilePickerFileType("CSV")
                                {
                                    Patterns = new[] { "*.csv" }
                                }
                            }
                        });

                    if (files.Count == 0) return;

                    var path = files[0].Path.LocalPath;
                    var imported = _csvService.ImportCandidates(path);

                    int added = 0;
                    int skipped = 0;

                    foreach (var candidate in imported)
                    {
                        if (await _service.IsDuplicateAsync(candidate.FullName, candidate.Telephone))
                        {
                            skipped++;
                            continue;
                        }

                        await _service.AddCandidateAsync(candidate);
                        added++;
                    }

                    await LoadCandidatesAsync();
                    await ShowInfoAsync($"Импорт завершён.\nДобавлено: {added}\nПропущено дубликатов: {skipped}");
                }
                catch (Exception ex)
                {
                    await ShowInfoAsync($"Ошибка импорта: {ex.Message}");
                }
            });
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

        /// <summary>
        /// Выполняет асинхронную операцию, показывая курсор ожидания.
        /// </summary>
        /// <param name="operation">Асинхронная операция.</param>
        private async Task RunWithWaitCursorAsync(Func<Task> operation)
        {
            var mainWindow = (Application.Current?.ApplicationLifetime
                as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

            if (mainWindow != null)
                mainWindow.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Wait);

            try
            {
                await operation();
            }
            finally
            {
                if (mainWindow != null)
                    mainWindow.Cursor = Avalonia.Input.Cursor.Default;
            }
        }

        /// <summary>
        /// Перезагружает списки кандидатов и вакансий после завершения собеседования.
        /// Вызывается из InterviewTabViewModel.
        /// </summary>
        public async Task RefreshAfterInterviewAsync()
        {
            await LoadCandidatesAsync();
            await VacancyTab.LoadVacanciesAsyncPublic();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Простая реализация ICommand для привязки действий к кнопкам.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _execute(parameter);
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

