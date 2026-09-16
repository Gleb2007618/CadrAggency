using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CadrAggency.Models;
using CadrAggency.Services;
using CadrAggency.Views;

namespace CadrAggency.ViewModels
{
    /// <summary>
    /// ViewModel вкладки «Вакансии».
    /// Управляет таблицей вакансий и операциями добавления,
    /// редактирования и удаления.
    /// </summary>
    public class VacancyTabViewModel : INotifyPropertyChanged
    {
        private readonly VacancyService _service;

        /// <summary>
        /// Конструктор. Сразу запускает загрузку списка вакансий.
        /// </summary>
        /// <param name="service">Сервис для работы с вакансиями.</param>
        public VacancyTabViewModel(VacancyService service)
        {
            _service = service;
            _ = LoadVacanciesAsync();
        }

        private ObservableCollection<Vacancy> _vacancies = new();
        public ObservableCollection<Vacancy> Vacancies
        {
            get => _vacancies;
            set { _vacancies = value; OnPropertyChanged(); }
        }

        private Vacancy? _selectedVacancy;
        public Vacancy? SelectedVacancy
        {
            get => _selectedVacancy;
            set
            {
                _selectedVacancy = value;
                OnPropertyChanged();
                _editCommand?.RaiseCanExecuteChanged();
                _deleteCommand?.RaiseCanExecuteChanged();
            }
        }

        private RelayCommand? _editCommand;
        private RelayCommand? _deleteCommand;

        public ICommand LoadCommand => new RelayCommand(async _ => await LoadVacanciesAsync());
        public ICommand AddCommand => new RelayCommand(async _ => await AddVacancyAsync());

        public ICommand EditCommand => _editCommand ??= new RelayCommand(
            async _ => await EditVacancyAsync(),
            _ => SelectedVacancy != null);

        public ICommand DeleteCommand => _deleteCommand ??= new RelayCommand(
            async _ => await DeleteVacancyAsync(),
            _ => SelectedVacancy != null);

        /// <summary>
        /// Публичный метод для внешнего обновления списка вакансий
        /// (например, после завершения собеседования).
        /// </summary>
        public Task LoadVacanciesAsyncPublic() => LoadVacanciesAsync();

        /// <summary>
        /// Загружает список вакансий из базы данных.
        /// </summary>
        private async Task LoadVacanciesAsync()
        {
            try
            {
                var list = await _service.GetAllVacanciesAsync();
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Vacancies.Clear();
                    foreach (var v in list)
                        Vacancies.Add(v);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки вакансий: {ex.Message}");
            }
        }

        /// <summary>
        /// Открывает диалог добавления новой вакансии.
        /// </summary>
        private async Task AddVacancyAsync()
        {
            var mainWindow = (Application.Current?.ApplicationLifetime
                as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow == null) return;

            var dialog = new VacancyDialog();
            var vm = new VacancyDialogViewModel(_service);
            dialog.DataContext = vm;

            vm.RequestClose += (s, e) => dialog.Close();
            vm.VacancySaved += async (s, vacancy) =>
            {
                try
                {
                    await _service.AddVacancyAsync(vacancy);
                    await LoadVacanciesAsync();
                }
                catch (Exception ex)
                {
                    await ShowInfoAsync($"Ошибка добавления вакансии: {ex.Message}");
                }
            };

            await dialog.ShowDialog(mainWindow);
        }

        /// <summary>
        /// Открывает диалог редактирования выбранной вакансии.
        /// </summary>
        private async Task EditVacancyAsync()
        {
            if (SelectedVacancy == null) return;

            var mainWindow = (Application.Current?.ApplicationLifetime
                as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow == null) return;

            var dialog = new VacancyDialog();
            var vm = new VacancyDialogViewModel(_service, SelectedVacancy);
            dialog.DataContext = vm;

            vm.RequestClose += (s, e) => dialog.Close();
            vm.VacancySaved += async (s, vacancy) =>
            {
                try
                {
                    await _service.UpdateVacancyAsync(vacancy);
                    await LoadVacanciesAsync();
                }
                catch (Exception ex)
                {
                    await ShowInfoAsync($"Ошибка редактирования вакансии: {ex.Message}");
                }
            };

            await dialog.ShowDialog(mainWindow);
        }

        /// <summary>
        /// Удаляет выбранную вакансию с подтверждением.
        /// Запрещает удаление, если на вакансию назначены собеседования.
        /// </summary>
        private async Task DeleteVacancyAsync()
        {
            if (SelectedVacancy == null) return;

            var mainWindow = (Application.Current?.ApplicationLifetime
                as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow == null) return;

            if (await _service.HasInterviewsAsync(SelectedVacancy.Id))
            {
                await ShowInfoAsync($"Нельзя удалить вакансию «{SelectedVacancy.Title}», на неё назначены собеседования.");
                return;
            }

            var confirmDialog = new ConfirmDialog();
            var confirmVm = new ConfirmDialogViewModel(
                $"Вы уверены, что хотите удалить вакансию «{SelectedVacancy.Title}»?");
            confirmDialog.DataContext = confirmVm;
            confirmVm.RequestClose += (s, e) => confirmDialog.Close();
            await confirmDialog.ShowDialog(mainWindow);

            if (!confirmVm.Result) return;

            try
            {
                await _service.DeleteVacancyAsync(SelectedVacancy.Id);
                await LoadVacanciesAsync();
            }
            catch (Exception ex)
            {
                await ShowInfoAsync($"Ошибка удаления вакансии: {ex.Message}");
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

