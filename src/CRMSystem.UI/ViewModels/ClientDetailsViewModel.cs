using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMSystem.Business.Services;
using CRMSystem.Domain.Entities;
using CRMSystem.UI.Services;

namespace CRMSystem.UI.ViewModels;

public partial class ClientDetailsViewModel : ViewModelBase, INavigationAware
{
    private readonly IClientService _clientService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private Client? _client;

    [ObservableProperty]
    private ObservableCollection<Contact> _contacts = new();

    [ObservableProperty]
    private Contact? _selectedContact;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public ClientDetailsViewModel(
        IClientService clientService,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _clientService = clientService;
        _navigationService = navigationService;
        _dialogService = dialogService;
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is int clientId)
            _ = LoadAsync(clientId);
    }

    private async Task LoadAsync(int clientId)
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading client...";

            var data = await _clientService.GetByIdWithContactsAsync(clientId);
            if (data == null)
            {
                StatusMessage = "Client not found.";
                MessageBox.Show("Client not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _navigationService.NavigateTo<ClientListViewModel>();
                return;
            }

            Client = data;
            Contacts = new ObservableCollection<Contact>(
                data.Contacts.OrderByDescending(c => c.Date));

            StatusMessage = $"Loaded {Contacts.Count} contact(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.NavigateTo<ClientListViewModel>();
    }

    [RelayCommand]
    private async Task EditClientAsync()
    {
        if (Client == null) return;

        var saved = _dialogService.ShowClientForm(Client);
        if (saved)
        {
            StatusMessage = "Client updated.";
            await LoadAsync(Client.Id);
        }
    }
}