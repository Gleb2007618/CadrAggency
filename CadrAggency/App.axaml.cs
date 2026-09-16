using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CadrAggency.Views;

namespace CadrAggency;

/// <summary>
/// Класс приложения Avalonia.
/// Отвечает за загрузку XAML-ресурсов и создание главного окна.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Инициализация приложения.
    /// Загружает стили и ресурсы из App.axaml.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Вызывается после завершения инициализации платформы.
    /// Создаёт и показывает главное окно приложения.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}

