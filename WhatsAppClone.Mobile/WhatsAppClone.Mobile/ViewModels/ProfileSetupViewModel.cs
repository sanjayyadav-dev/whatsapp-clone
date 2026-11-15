using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WhatsAppClone.Mobile.Services;

namespace WhatsAppClone.Mobile.ViewModels;

public class ProfileSetupViewModel : ObservableObject
{
    private readonly AuthService _auth;

    private string _displayName = "";
    private string _about = "Hey there! I am using WhatsApp Clone.";
    private string _error = "";
    private bool _isBusy;

    public string DisplayName { get => _displayName; set => SetProperty(ref _displayName, value); }
    public string About { get => _about; set => SetProperty(ref _about, value); }
    public string Error { get => _error; set => SetProperty(ref _error, value); }
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

    public IAsyncRelayCommand SaveCommand { get; }

    public ProfileSetupViewModel(AuthService auth)
    {
        _auth = auth;
        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    private async Task SaveAsync()
    {
        Error = "";
        if (string.IsNullOrWhiteSpace(DisplayName)) { Error = "Name is required."; return; }

        IsBusy = true;
        var r = await _auth.UpdateProfileAsync(DisplayName.Trim(), About?.Trim());
        IsBusy = false;

        if (!r.Success) { Error = r.Error ?? "Failed."; return; }
        await Shell.Current.GoToAsync("//MainPage");
    }
}