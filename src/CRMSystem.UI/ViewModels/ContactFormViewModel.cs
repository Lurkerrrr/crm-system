using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CRMSystem.Business.Services;
using CRMSystem.Domain.Entities;
using CRMSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CRMSystem.UI.ViewModels;

public enum ContactFormMode
{
    Add,
    Edit
}

public partial class ContactFormViewModel : ObservableValidator
{
    private readonly IContactService _contactService;

    private int _editingContactId;
    private int _clientId;

    [ObservableProperty]
    private ContactFormMode _mode = ContactFormMode.Add;

    [ObservableProperty]
    private DateTime _date = DateTime.Now;

    [ObservableProperty]
    private ContactType _selectedType = ContactType.Note;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Description is required.")]
    [MinLength(3, ErrorMessage = "Description must be at least 3 characters.")]
    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public event EventHandler<bool>? CloseRequested;

    public IReadOnlyList<ContactType> AvailableTypes { get; } =
        Enum.GetValues<ContactType>();

    public string Title => Mode == ContactFormMode.Add ? "Add New Contact" : "Edit Contact";

    public ContactFormViewModel(IContactService contactService)
    {
        _contactService = contactService;
    }

    /// <summary>
    /// Loads the form for Add (pass clientId) or Edit (pass existing contact).
    /// </summary>
    public void Load(int clientId, Contact? contact)
    {
        _clientId = clientId;

        if (contact == null)
        {
            Mode = ContactFormMode.Add;
            _editingContactId = 0;
            Date = DateTime.Now;
            SelectedType = ContactType.Note;
            Description = string.Empty;
        }
        else
        {
            Mode = ContactFormMode.Edit;
            _editingContactId = contact.Id;
            Date = contact.Date;
            SelectedType = contact.Type;
            Description = contact.Description;
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

            var contact = new Contact
            {
                Id = _editingContactId,
                ClientId = _clientId,
                Date = Date,
                Type = SelectedType,
                Description = Description.Trim()
            };

            if (Mode == ContactFormMode.Add)
                await _contactService.CreateAsync(contact);
            else
                await _contactService.UpdateAsync(contact);

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