using CRMSystem.Domain.Entities;

namespace CRMSystem.UI.Services;

/// <summary>
/// Abstracts dialog/window operations away from ViewModels.
/// Keeps MVVM clean — VMs don't reference Window types directly.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows the client form. Pass null for Add mode, an existing client for Edit mode.
    /// Returns true if the user saved, false if cancelled.
    /// </summary>
    bool ShowClientForm(Client? client);

    bool ConfirmAction(string title, string message);
}