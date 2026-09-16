using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CadrAggency.Models;
using CadrAggency.Models.Enums;

namespace CadrAggency.ViewModels
{
    /// <summary>
    /// ViewModel диалога завершения собеседования.
    /// Позволяет выбрать результат и оставить комментарий рекрутера.
    /// </summary>
    public class CompleteInterviewDialogViewModel : INotifyPropertyChanged
    {
        private readonly Interview _interview;

        /// <summary>
        /// Конструктор. Предзаполняет поля данными собеседования,
        /// если результат уже был выставлен ранее.
        /// </summary>
        /// <param name="interview">Завершаемое собеседование.</param>
        public CompleteInterviewDialogViewModel(Interview interview)
        {
            _interview = interview;

            if (interview.Result.HasValue)
                Result = interview.Result.Value;

            Comment = interview.Comment ?? string.Empty;

            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(this, EventArgs.Empty));
        }

        /// <summary>Заголовок окна с именем соискателя и названием вакансии.</summary>
        public string HeaderText =>
            $"Собеседование: {_interview.Candidate?.FullName} — {_interview.Vacancy?.Title}";

        /// <summary>Список возможных результатов для ComboBox.</summary>
        public List<InterviewResult> Results =>
            Enum.GetValues(typeof(InterviewResult)).Cast<InterviewResult>().ToList();

        private InterviewResult _result = InterviewResult.Successful;
        public InterviewResult Result
        {
            get => _result;
            set { _result = value; OnPropertyChanged(); }
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

        /// <summary>Событие завершения собеседования с заполненным результатом.</summary>
        public event EventHandler<Interview>? InterviewCompleted;

        /// <summary>
        /// Записывает результат и комментарий в объект собеседования
        /// и передаёт его через событие InterviewCompleted.
        /// </summary>
        private void Save()
        {
            _interview.Result = Result;
            _interview.Comment = string.IsNullOrWhiteSpace(Comment) ? null : Comment;

            InterviewCompleted?.Invoke(this, _interview);
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

