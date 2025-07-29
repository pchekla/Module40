using Xamarin.Forms;
using Module40.ViewModels;

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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Обновляем состояние ViewModel каждый раз при появлении страницы
            if (BindingContext is PinViewModel viewModel)
            {
                viewModel.RefreshState();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            // Очищаем поле PIN при уходе со страницы для безопасности
            if (BindingContext is PinViewModel viewModel)
            {
                viewModel.Pin = string.Empty;
            }
        }
    }
}
