using System;
using System.Windows;
using ROV_GUI_Control.ViewModels;

namespace ROV_GUI_Control
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            var windowService = new WindowService();
            ViewModel = new MainViewModel("rov","rov2025", windowService);
            DataContext = ViewModel;
        }
        protected override void OnClosed(EventArgs e)
        {
            ViewModel.Dispose();
            base.OnClosed(e);
        }
    }
}