using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMSystem.Business.Exceptions;
using CRMSystem.Business.Services;
using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CRMSystem.UI.ViewModels;

public enum ClientFormMode
{
    Add,
    Edit
}

/// <summary>
/// ViewModel for the Client Add/Edit form. Works in both modes.
/// </summary>
public partial class ClientFormViewModel : ObservableValidator
{
    private readonly IClientService _clientService;

    private int _editingClientId;

    [ObservableProperty]
    private ClientFormMode _mode = ClientFormMode.Add;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "First name is required.")]
    [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
    private string _firstName = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Last name is required.")]
    [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
    private string _lastName = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [MaxLength(200, ErrorMessage = "Company name cannot exceed 200 characters.")]
    private string? _company;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email format is invalid.")]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
    private string _email = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [MaxLength(50, ErrorMessage = "Phone cannot exceed 50 characters.")]
    private string? _phone;

    [ObservableProperty]
    private ClientStatus _selectedStatus = ClientStatus.New;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    /// <summary>
    /// Fires when the form is successfully saved or cancelled.
    /// True = saved (close with success), False = cancelled, null = form still open.
    /// </summary>
    public event EventHandler<bool>? CloseRequested;

    public IReadOnlyList<ClientStatus> AvailableStatuses { get; } =
        Enum.GetValues<ClientStatus>();

    public string Title => Mode == ClientFormMode.Add ? "Add New Client" : "Edit Client";

    public ClientFormViewModel(IClientService clientService)
    {
        _clientService = clientService;
    }

    /// <summary>
    /// Loads the form for Add (pass null) or Edit (pass an existing client).
    /// </summary>
    public void Load(Client? client)
    {
        if (client == null)
        {
            Mode = ClientFormMode.Add;
            _editingClientId = 0;
            FirstName = string.Empty;
            LastName = string.Empty;
            Company = null;
            Email = string.Empty;
            Phone = null;
            SelectedStatus = ClientStatus.New;
        }
        else
        {
            Mode = ClientFormMode.Edit;
            _editingClientId = client.Id;
            FirstName = client.FirstName;
            LastName = client.LastName;
            Company = client.Company;
            Email = client.Email;
            Phone = client.Phone;
            SelectedStatus = client.Status;
        }

        ClearErrors();
        StatusMessage = string.Empty;
        OnPropertyChanged(nameof(Title));
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        ValidateAllProperties();
        if (HasErrors)
        {
            StatusMessage = "Please fix the errors above.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Saving...";

            var client = new Client
            {
                Id = _editingClientId,
                FirstName = FirstName.Trim(),
                LastName = LastName.Trim(),
                Company = string.IsNullOrWhiteSpace(Company) ? null : Company.Trim(),
                Email = Email.Trim(),
                Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
                Status = SelectedStatus
            };

            if (Mode == ClientFormMode.Add)
                await _clientService.CreateAsync(client);
            else
                await _clientService.UpdateAsync(client);

            CloseRequested?.Invoke(this, true);
        }
        catch (CRMSystem.Business.Exceptions.ValidationException vex)
        {
            StatusMessage = string.Join(Environment.NewLine, vex.Errors);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke(this, false);
    }

    private bool CanSave() => !IsBusy;

    partial void OnIsBusyChanged(bool value)
    {
        SaveCommand.NotifyCanExecuteChanged();
    }
}