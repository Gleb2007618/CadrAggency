using Avalonia;
using System;
using CadrAggency.Data;

namespace CadrAggency
{
    /// <summary>
    /// Точка входа приложения.
    /// Отвечает за инициализацию базы данных и запуск Avalonia-приложения.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Главный метод приложения.
        /// Проверяет/создаёт базу данных и запускает графический интерфейс.
        /// </summary>
        /// <param name="args">Аргументы командной строки.</param>
        public static void Main(string[] args)
        {
            using (var db = new HrDbContext())
            {
                db.Database.EnsureCreated();
                Console.WriteLine("База данных проверена/создана.");
            }

            try
            {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ОШИБКА ЗАПУСКА: {ex.Message} ===");
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Создаёт и настраивает конфигурацию Avalonia-приложения.
        /// Указывает платформенные настройки, шрифты и логирование.
        /// </summary>
        /// <returns>Настроенный объект AppBuilder.</returns>
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}


