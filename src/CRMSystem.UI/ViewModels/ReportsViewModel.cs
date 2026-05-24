using CommunityToolkit.Mvvm.ComponentModel;

namespace CRMSystem.UI.ViewModels;

public partial class ReportsViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _statusMessage = "Reports view — charts coming in a later phase.";
}