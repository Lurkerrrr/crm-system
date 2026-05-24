using System.Windows;
using CRMSystem.UI.ViewModels;

namespace CRMSystem.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}