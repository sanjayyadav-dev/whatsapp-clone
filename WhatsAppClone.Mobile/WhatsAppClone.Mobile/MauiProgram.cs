using Microsoft.Extensions.Logging;
using WhatsAppClone.Mobile.Services;
using WhatsAppClone.Mobile.ViewModels;
using WhatsAppClone.Mobile.Views;

namespace WhatsAppClone.Mobile;

public static class MauiProgram
{
    // API ka https port: API project ki Properties/launchSettings.json me
    // "https" profile ke applicationUrl se dekho (jaise https://localhost:7123)
    private const int ApiPort = 7047;

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Android emulator me PC ka localhost = 10.0.2.2
        var host = DeviceInfo.Platform == DevicePlatform.Android ? "10.0.2.2" : "localhost";

        builder.Services.AddSingleton<TokenStore>();
        builder.Services.AddSingleton(_ =>
        {
            var handler = new HttpClientHandler();
#if DEBUG
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;   // dev certificate
#endif
            return new HttpClient(handler) { BaseAddress = new Uri($"https://{host}:{ApiPort}/") };
        });
        builder.Services.AddSingleton<ApiClient>();
        builder.Services.AddSingleton<AuthService>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<OtpViewModel>();
        builder.Services.AddTransient<ProfileSetupViewModel>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<OtpPage>();
        builder.Services.AddTransient<ProfileSetupPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}