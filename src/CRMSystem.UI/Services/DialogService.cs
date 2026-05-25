using System.Windows;
using CRMSystem.Domain.Entities;
using CRMSystem.UI.ViewModels;
using CRMSystem.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace CRMSystem.UI.Services;

public class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;

    public DialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public bool ShowClientForm(Client? client)
    {
        // Resolve a fresh VM and View from the DI container
        var viewModel = _serviceProvider.GetRequiredService<ClientFormViewModel>();
        viewModel.Load(client);

        var view = new ClientFormView(viewModel)
        {
            Owner = Application.Current.MainWindow
        };

        return view.ShowDialog() == true;
    }

    public bool ConfirmAction(string title, string message)
    {
        var result = MessageBox.Show(
            message,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        return result == MessageBoxResult.Yes;
    }

    public bool ShowContactForm(int clientId, Contact? contact)
    {
        var viewModel = _serviceProvider.GetRequiredService<ContactFormViewModel>();
        viewModel.Load(clientId, contact);

        var view = new ContactFormView(viewModel)
        {
            Owner = Application.Current.MainWindow
        };

        return view.ShowDialog() == true;
    }
}