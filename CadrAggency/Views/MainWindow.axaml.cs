using Avalonia.Controls;
using CadrAggency.Data;
using CadrAggency.Services;
using CadrAggency.ViewModels;

namespace CadrAggency.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var service = new CandidateService(new HrDbContext());
            this.DataContext = new MainWindowViewModel(service);
        }
    }
}


