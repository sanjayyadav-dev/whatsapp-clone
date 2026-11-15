using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WhatsAppClone.Mobile.Services;
using WhatsAppClone.Mobile.Views;

namespace WhatsAppClone.Mobile.ViewModels;

[QueryProperty(nameof(Phone), "phone")]
public class OtpViewModel : ObservableObject
{
    private readonly AuthService _auth;

    private string _phone = "";
    private string _otp = "";
    private string _error = "";
    private bool _isBusy;

    public string Phone { get => _phone; set => SetProperty(ref _phone, value); }
    public string Otp { get => _otp; set => SetProperty(ref _otp, value); }
    public string Error { get => _error; set => SetProperty(ref _error, value); }
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

    public IAsyncRelayCommand VerifyCommand { get; }
    public IAsyncRelayCommand ResendCommand { get; }

    public OtpViewModel(AuthService auth)
    {
        _auth = auth;
        VerifyCommand = new AsyncRelayCommand(VerifyAsync);
        ResendCommand = new AsyncRelayCommand(ResendAsync);
    }

    private async Task VerifyAsync()
    {
        Error = "";
        if (Otp.Length != 6) { Error = "Enter 6 digit OTP."; return; }

        IsBusy = true;
        var r = await _auth.VerifyOtpAsync(Phone, Otp);
        IsBusy = false;

        if (!r.Success || r.Data is null) { Error = r.Error ?? "Failed."; return; }

        if (r.Data.IsNewUser || string.IsNullOrEmpty(r.Data.User.DisplayName))
            await Shell.Current.GoToAsync(nameof(ProfileSetupPage));
        else
            await Shell.Current.GoToAsync("//MainPage");
    }

    private async Task ResendAsync()
    {
        var r = await _auth.SendOtpAsync(Phone);
        Error = r.Success ? "OTP sent again." : r.Error ?? "Failed.";
    }
}