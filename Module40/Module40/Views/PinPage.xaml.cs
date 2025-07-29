using Xamarin.Forms;

namespace Module40.Views
{
    public partial class PinPage
    {
        public PinPage()
        {
            InitializeComponent();
            MessagingCenter.Subscribe<ViewModels.PinViewModel>(this, "PinSuccess", async (sender) =>
            {
                // Навигация на экран изображений
                await Navigation.PushAsync(new ImagesPage());
            });
        }
    }
}

