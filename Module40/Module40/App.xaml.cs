using Module40.Views;
using Xamarin.Forms;

namespace Module40
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            // Проверяем, установлен ли PIN-код
            MainPage = new NavigationPage(new PinPage());
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
