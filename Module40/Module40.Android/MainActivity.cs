using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;

namespace Module40.Droid
{
    [Activity(Label = "Module40", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            // Просто запускаем приложение, разрешения будем обрабатывать внутри приложения
            LoadApplication(new App());
        }

        // Делегат для передачи результата запроса разрешений
        public System.Action<int, string[], Android.Content.PM.Permission[]> RequestPermissionsResultReceived { get; set; }

        // Оставляем метод для корректной работы Xamarin.Essentials
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            // Вызов делегата для PermissionService
            RequestPermissionsResultReceived?.Invoke(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}