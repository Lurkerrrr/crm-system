using System.Windows;
using CRMSystem.Business.Services;
using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Enums;

namespace CRMSystem.UI.Views;

public partial class MainWindow : Window
{
    private readonly IClientService _clientService;

    public MainWindow(IClientService clientService)
    {
        _clientService = clientService;
        InitializeComponent();
    }

    private async void SeedDataButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var existing = await _clientService.GetAllAsync();
            if (existing.Any())
            {
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

            foreach (var client in samples)
                await _clientService.CreateAsync(client);

            MessageBox.Show(
                $"Seeded {samples.Length} sample clients.",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void LoadClientsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var clients = await _clientService.GetAllAsync();
            ClientsGrid.ItemsSource = clients;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error loading clients: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}