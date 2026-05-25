using System.Collections.ObjectModel;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMSystem.Business.Exceptions;
using CRMSystem.Business.Services;
using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Enums;
using CRMSystem.UI.Services;

namespace CRMSystem.UI.ViewModels;

public partial class ClientListViewModel : ViewModelBase
{
    private readonly IClientService _clientService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<Client> _clients = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    public ICollectionView ClientsView { get; private set; } = null!;

    [ObservableProperty]
    private Client? _selectedClient;

    [ObservableProperty]
    private string _statusMessage = "Ready.";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private StatusFilterOption _selectedStatusFilter;

    public IReadOnlyList<StatusFilterOption> AvailableStatusFilters { get; }

    partial void OnSelectedStatusFilterChanged(StatusFilterOption value)
    {
        ClientsView?.Refresh();
        UpdateFilteredCount();
    }

    public ClientListViewModel(
        IClientService clientService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _clientService = clientService;
        _dialogService = dialogService;
        _navigationService = navigationService;

        // Build the filter options: "All statuses" + one per enum value
        var options = new List<StatusFilterOption>
        {
            new("All statuses", null)
        };
        options.AddRange(Enum.GetValues<ClientStatus>()
            .Select(s => new StatusFilterOption(s.ToString(), s)));
        AvailableStatusFilters = options;

        _selectedStatusFilter = AvailableStatusFilters[0];

        ClientsView = CollectionViewSource.GetDefaultView(Clients);
        ClientsView.Filter = FilterClient;
        _ = LoadClientsAsync();
    }

    [RelayCommand(CanExecute = nameof(CanRunCommand))]
    private async Task LoadClientsAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading clients...";
            var data = await _clientService.GetAllAsync();
            Clients = new ObservableCollection<Client>(data);
            StatusMessage = $"Loaded {Clients.Count} client(s).";
            UpdateFilteredCount();
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

    [RelayCommand(CanExecute = nameof(CanRunCommand))]
    private async Task SeedDataAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Seeding sample data...";

            var existing = await _clientService.GetAllAsync();
            if (existing.Any())
            {
                StatusMessage = "Database already contains clients. Skipping seed.";
                MessageBox.Show(
                    "Database already contains clients. Skipping seed.",
                    "Info",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var samples = new[]
            {
                new Client { FirstName = "Anna", LastName = "Kowalska", Company = "TechSoft Sp. z o.o.", Email = "anna.kowalska@techsoft.pl", Phone = "+48 600 100 200", Status = ClientStatus.Active },
                new Client { FirstName = "Jan", LastName = "Nowak", Company = "BuildCorp", Email = "jan.nowak@buildcorp.pl", Phone = "+48 601 300 400", Status = ClientStatus.New },
                new Client { FirstName = "Maria", LastName = "Wiśniewska", Company = "GreenLeaf", Email = "m.wisniewska@greenleaf.pl", Status = ClientStatus.InNegotiation },
                new Client { FirstName = "Piotr", LastName = "Zieliński", Company = null, Email = "piotr.z@gmail.com", Status = ClientStatus.New },
                new Client { FirstName = "Katarzyna", LastName = "Lewandowska", Company = "MediaPro", Email = "katarzyna@mediapro.pl", Phone = "+48 602 500 600", Status = ClientStatus.Closed }
            };

            foreach (var c in samples)
                await _clientService.CreateAsync(c);

            StatusMessage = $"Seeded {samples.Length} sample clients.";
            await LoadClientsAsync();
        }
        catch (ValidationException vex)
        {
            StatusMessage = "Validation failed.";
            MessageBox.Show(
                string.Join(Environment.NewLine, vex.Errors),
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
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
    private async Task AddClientAsync()
    {
        var saved = _dialogService.ShowClientForm(null);
        if (saved)
        {
            StatusMessage = "Client added.";
            await LoadClientsAsync();
        }
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedStatusFilter = AvailableStatusFilters[0];
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private void ViewDetails()
    {
        if (SelectedClient == null) return;
        _navigationService.NavigateTo<ClientDetailsViewModel>(SelectedClient.Id);
    }

    private bool CanEditOrDelete() => SelectedClient != null;

    partial void OnSelectedClientChanged(Client? value)
    {
        ViewDetailsCommand.NotifyCanExecuteChanged();
        DeleteClientCommand.NotifyCanExecuteChanged();
    }

    private bool CanRunCommand() => !IsBusy;

    partial void OnIsBusyChanged(bool value)
    {
        LoadClientsCommand.NotifyCanExecuteChanged();
        SeedDataCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task DeleteClientAsync()
    {
        if (SelectedClient == null) return;

        var confirmed = _dialogService.ConfirmAction(
            "Delete Client",
            $"Are you sure you want to delete {SelectedClient.FirstName} {SelectedClient.LastName}?\n\n" +
            "This will also delete all associated contacts. This action cannot be undone.");

        if (!confirmed) return;

        try
        {
            IsBusy = true;
            StatusMessage = "Deleting...";
            await _clientService.DeleteAsync(SelectedClient.Id);
            StatusMessage = "Client deleted.";
            await LoadClientsAsync();
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

    private bool FilterClient(object obj)
    {
        if (obj is not Client client) return false;

        // Text filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var term = SearchText.Trim().ToLowerInvariant();
            var matches =
                client.FirstName.ToLowerInvariant().Contains(term) ||
                client.LastName.ToLowerInvariant().Contains(term) ||
                (client.Company?.ToLowerInvariant().Contains(term) ?? false) ||
                client.Email.ToLowerInvariant().Contains(term);

            if (!matches) return false;
        }

        // Status filter
        if (SelectedStatusFilter?.Status.HasValue == true
            && client.Status != SelectedStatusFilter.Status.Value)
            return false;

        return true;
    }

    private void UpdateFilteredCount()
    {
        if (ClientsView == null) return;
        var visibleCount = ClientsView.Cast<Client>().Count();
        var totalCount = Clients.Count;

        if (visibleCount == totalCount)
            StatusMessage = $"Showing all {totalCount} client(s).";
        else
            StatusMessage = $"Showing {visibleCount} of {totalCount} client(s).";
    }

    partial void OnSearchTextChanged(string value)
    {
        ClientsView?.Refresh();
        UpdateFilteredCount();
    }


    partial void OnClientsChanged(ObservableCollection<Client> value)
    {
        ClientsView = CollectionViewSource.GetDefaultView(value);
        ClientsView.Filter = FilterClient;
        OnPropertyChanged(nameof(ClientsView));
    }
}

public record StatusFilterOption(string Label, ClientStatus? Status)
{
    public override string ToString() => Label;
}