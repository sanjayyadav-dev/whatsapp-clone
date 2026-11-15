using WhatsAppClone.Mobile.ViewModels;

namespace WhatsAppClone.Mobile.Views;

public partial class ProfileSetupPage : ContentPage
{
    public ProfileSetupPage(ProfileSetupViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}