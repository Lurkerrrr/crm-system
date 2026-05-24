using CRMSystem.UI.ViewModels;

namespace CRMSystem.UI.Services;

/// <summary>
/// Provides navigation between ViewModels within the main shell window.
/// </summary>
public interface INavigationService
{
    ViewModelBase? CurrentViewModel { get; }

    event EventHandler? CurrentViewModelChanged;

    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
}