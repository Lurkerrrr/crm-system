using CRMSystem.UI.ViewModels;

namespace CRMSystem.UI.Services;

public interface INavigationService
{
    ViewModelBase? CurrentViewModel { get; }

    event EventHandler? CurrentViewModelChanged;

    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;

    /// <summary>
    /// Navigates to a ViewModel and passes a parameter to it via INavigationAware.
    /// </summary>
    void NavigateTo<TViewModel>(object parameter) where TViewModel : ViewModelBase;
}

/// <summary>
/// Implemented by ViewModels that need a parameter on navigation (e.g. an ID).
/// </summary>
public interface INavigationAware
{
    void OnNavigatedTo(object parameter);
}