using System.Windows;
using CRMSystem.UI.ViewModels;

namespace CRMSystem.UI.Views;

public partial class ContactFormView : Window
{
    public ContactFormView(ContactFormViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseRequested += OnCloseRequested;
    }

    private void OnCloseRequested(object? sender, bool saved)
    {
        DialogResult = saved;
        Close();
    }
}