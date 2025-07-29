using Xamarin.Forms;

namespace Module40.Views
{
    public partial class PinPage : ContentPage
    {
        public PinPage()
        {
            InitializeComponent();
            Xamarin.Forms.MessagingCenter.Subscribe<ViewModels.PinViewModel>(this, "PinSuccess", async (sender) =>
            {
                // Навигация на экран изображений
                await Navigation.PushAsync(new ImagesPage());
            });
        }
    }
}

