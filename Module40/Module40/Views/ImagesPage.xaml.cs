using Xamarin.Forms;

namespace Module40.Views
{
    public partial class ImagesPage
    {
        public ImagesPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Subscribe<ViewModels.ImagesViewModel, string>(this, ViewModels.ImagesViewModel.PermissionDeniedAlert, async (sender, msg) =>
            {
                await DisplayAlert("Внимание", msg, "OK");
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<ViewModels.ImagesViewModel, string>(this, ViewModels.ImagesViewModel.PermissionDeniedAlert);
        }
    }
}
