using CRMSystem.Domain.Entities;

namespace CRMSystem.UI.Services;

public interface IDialogService
{
    bool ShowClientForm(Client? client);
    bool ShowContactForm(int clientId, Contact? contact);
    bool ConfirmAction(string title, string message);

    /// <summary>
    /// Shows a Save File dialog. Returns the chosen path, or null if cancelled.
    /// </summary>
    string? ShowSaveFileDialog(string title, string defaultFileName, string filter);

    void ShowInformation(string title, string message);
}