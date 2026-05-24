using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMSystem.Business.Exceptions;
using CRMSystem.Business.Services;
using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Enums;

namespace CRMSystem.UI.ViewModels;

/// <summary>
/// ViewModel for the main window. Manages the client list and seed/load commands.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly IClientService _clientService;

    [ObservableProperty]
    private ObservableCollection<Client> _clients = new();

    [ObservableProperty]
    private Client? _selectedClient;

    [ObservableProperty]
    private string _statusMessage = "Ready.";

    [ObservableProperty]
    private bool _isBusy;

    public MainViewModel(IClientService clientService)
    {
        _clientService = clientService;
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

            // Auto-refresh after seeding
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

    private bool CanRunCommand() => !IsBusy;

    // When IsBusy changes, re-evaluate command availability
    partial void OnIsBusyChanged(bool value)
    {
        LoadClientsCommand.NotifyCanExecuteChanged();
        SeedDataCommand.NotifyCanExecuteChanged();
    }
}