using CRMSystem.Domain.Entities;

namespace CRMSystem.UI.Services;

public interface IDialogService
{
    bool ShowClientForm(Client? client);
    bool ShowContactForm(int clientId, Contact? contact);
    bool ConfirmAction(string title, string message);
}