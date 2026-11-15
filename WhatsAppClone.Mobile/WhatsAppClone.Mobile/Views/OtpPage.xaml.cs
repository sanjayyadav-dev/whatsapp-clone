using WhatsAppClone.Mobile.ViewModels;

namespace WhatsAppClone.Mobile.Views;

public partial class OtpPage : ContentPage
{
    public OtpPage(OtpViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}