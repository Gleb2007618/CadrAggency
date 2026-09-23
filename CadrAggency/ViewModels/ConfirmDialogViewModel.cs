using System;
using System.Windows.Input;

namespace CadrAggency.ViewModels
{
    /// <summary>
    /// ViewModel универсального диалога подтверждения.
    /// Используется как для вопросов «Да/Нет», так и для информационных сообщений.
    /// </summary>
    public class ConfirmDialogViewModel
    {
        /// <summary>Текст сообщения для отображения.</summary>
        public string Message { get; }

        /// <summary>Команда «Да» (подтверждение).</summary>
        public ICommand YesCommand { get; }

        /// <summary>Команда «Нет» (отмена).</summary>
        public ICommand NoCommand { get; }

        /// <summary>Событие запроса на закрытие диалогового окна.</summary>
        public event EventHandler? RequestClose;

        /// <summary>Результат выбора: true — Да, false — Нет.</summary>
        public bool Result { get; private set; }

        /// <summary>
        /// Конструктор. Принимает текст сообщения.
        /// </summary>
        /// <param name="message">Сообщение для отображения.</param>
        public ConfirmDialogViewModel(string message)
        {
            Message = message;
            YesCommand = new RelayCommand(_ =>
            {
                Result = true;
                RequestClose?.Invoke(this, EventArgs.Empty);
            });
            NoCommand = new RelayCommand(_ =>
            {
                Result = false;
                RequestClose?.Invoke(this, EventArgs.Empty);
            });
        }
    }
}



