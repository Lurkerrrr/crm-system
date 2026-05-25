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
    private readonly IContactService _contactService;
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
        IContactService contactService,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _clientService = clientService;
        _contactService = contactService;
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

    [RelayCommand]
    private async Task AddContactAsync()
    {
        if (Client == null) return;

        var saved = _dialogService.ShowContactForm(Client.Id, null);
        if (saved)
        {
            StatusMessage = "Contact added.";
            await LoadAsync(Client.Id);
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDeleteContact))]
    private async Task EditContactAsync()
    {
        if (Client == null || SelectedContact == null) return;

        var saved = _dialogService.ShowContactForm(Client.Id, SelectedContact);
        if (saved)
        {
            StatusMessage = "Contact updated.";
            await LoadAsync(Client.Id);
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDeleteContact))]
    private async Task DeleteContactAsync()
    {
        if (SelectedContact == null || Client == null) return;

        var confirmed = _dialogService.ConfirmAction(
            "Delete Contact",
            $"Are you sure you want to delete this {SelectedContact.Type} from {SelectedContact.Date:yyyy-MM-dd}?\n\n" +
            "This action cannot be undone.");

        if (!confirmed) return;

        try
        {
            IsBusy = true;
            StatusMessage = "Deleting...";
            await _contactService.DeleteAsync(SelectedContact.Id);
            StatusMessage = "Contact deleted.";
            await LoadAsync(Client.Id);
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

    private bool CanEditOrDeleteContact() => SelectedContact != null;

    partial void OnSelectedContactChanged(Contact? value)
    {
        EditContactCommand.NotifyCanExecuteChanged();
        DeleteContactCommand.NotifyCanExecuteChanged();
    }
}