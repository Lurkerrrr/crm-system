using CommunityToolkit.Mvvm.ComponentModel;

namespace CRMSystem.UI.ViewModels;

/// <summary>
/// Base class for all ViewModels in the application.
/// Provides INotifyPropertyChanged via ObservableObject.
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
}