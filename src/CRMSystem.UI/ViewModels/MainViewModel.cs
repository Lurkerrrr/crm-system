using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMSystem.UI.Services;

namespace CRMSystem.UI.ViewModels;

/// <summary>
/// Shell ViewModel. Hosts the currently active page and exposes navigation commands.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ViewModelBase? _currentViewModel;

    public MainViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        _navigationService.CurrentViewModelChanged += OnNavigationChanged;

        // Start on the dashboard
        _navigationService.NavigateTo<DashboardViewModel>();
    }

    private void OnNavigationChanged(object? sender, EventArgs e)
    {
        CurrentViewModel = _navigationService.CurrentViewModel;
    }

    [RelayCommand]
    private void GoToDashboard() => _navigationService.NavigateTo<DashboardViewModel>();

    [RelayCommand]
    private void GoToClients() => _navigationService.NavigateTo<ClientListViewModel>();

    [RelayCommand]
    private void GoToReports() => _navigationService.NavigateTo<ReportsViewModel>();
}