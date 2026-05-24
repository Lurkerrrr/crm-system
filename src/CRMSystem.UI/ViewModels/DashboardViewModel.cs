using CommunityToolkit.Mvvm.ComponentModel;
using CRMSystem.Business.Services;

namespace CRMSystem.UI.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IReportService _reportService;

    [ObservableProperty]
    private int _totalClients;

    [ObservableProperty]
    private int _totalContacts;

    [ObservableProperty]
    private int _contactsLast30Days;

    [ObservableProperty]
    private string _statusMessage = "Welcome to CRM System.";

    public DashboardViewModel(IReportService reportService)
    {
        _reportService = reportService;
        _ = LoadStatsAsync();
    }

    private async Task LoadStatsAsync()
    {
        try
        {
            TotalClients = await _reportService.GetTotalClientCountAsync();
            TotalContacts = await _reportService.GetTotalContactCountAsync();
            ContactsLast30Days = await _reportService.GetContactsInLastDaysAsync(30);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load stats: {ex.Message}";
        }
    }
}