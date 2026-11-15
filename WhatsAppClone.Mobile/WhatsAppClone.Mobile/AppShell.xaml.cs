using WhatsAppClone.Mobile.Views;

namespace WhatsAppClone.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(OtpPage), typeof(OtpPage));
        Routing.RegisterRoute(nameof(ProfileSetupPage), typeof(ProfileSetupPage));
    }
}