using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WhatsAppClone.Mobile.Services;
using WhatsAppClone.Mobile.Views;

namespace WhatsAppClone.Mobile.ViewModels;

public class LoginViewModel : ObservableObject
{
    private readonly AuthService _auth;
    private readonly TokenStore _tokens;

    private string _phone = "";
    private string _error = "";
    private bool _isBusy;

    public string Phone { get => _phone; set => SetProperty(ref _phone, value); }
    public string Error { get => _error; set => SetProperty(ref _error, value); }
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

    public IAsyncRelayCommand SendOtpCommand { get; }

    public LoginViewModel(AuthService auth, TokenStore tokens)
    {
        _auth = auth;
        _tokens = tokens;
        SendOtpCommand = new AsyncRelayCommand(SendOtpAsync);
    }

    // App start par: token ho to seedha MainPage
    public async Task CheckSessionAsync()
    {
        if (await _tokens.GetAccessAsync() is null) return;
        var me = await _auth.GetMeAsync();   // 401 par auto-refresh
        if (me.Success) await Shell.Current.GoToAsync("//MainPage");
    }

    private async Task SendOtpAsync()
    {
        Error = "";
        var digits = new string(Phone.Where(char.IsDigit).ToArray());
        var full = digits.Length == 10 ? "+91" + digits : "+" + digits;   // 10 digit = India

        IsBusy = true;
        var r = await _auth.SendOtpAsync(full);
        IsBusy = false;

        if (!r.Success) { Error = r.Error ?? "Failed."; return; }
        await Shell.Current.GoToAsync($"{nameof(OtpPage)}?phone={Uri.EscapeDataString(full)}");
    }
}