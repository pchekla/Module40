using System.Threading.Tasks;
using Android;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Xamarin.Forms;
using Module40.Services;
using System.Linq;
using System.Collections.Generic;

[assembly: Dependency(typeof(Module40.Droid.PermissionService))]
namespace Module40.Droid
{
    public class PermissionService : Java.Lang.Object, IPermissionService
    {
        private TaskCompletionSource<bool> tcs;
        
        public Task<bool> CheckAndRequestStoragePermissionAsync()
        {
            var activity = Xamarin.Essentials.Platform.CurrentActivity;
            
            // Определяем нужные разрешения в зависимости от версии Android
            var permissions = new List<string>();
            
            // Камера нужна всегда
            permissions.Add(Manifest.Permission.Camera);
            
            // Для Android 13+ (API 33+) используем новые разрешения
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Tiramisu)
            {
                permissions.Add(Manifest.Permission.ReadMediaImages);
                permissions.Add(Manifest.Permission.ReadMediaVideo);
            }
            else
            {
                // Для более старых версий используем READ_EXTERNAL_STORAGE
                permissions.Add(Manifest.Permission.ReadExternalStorage);
            }
            
            // WRITE_EXTERNAL_STORAGE нужен для удаления/изменения файлов
            if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.R) // API < 30
            {
                permissions.Add(Manifest.Permission.WriteExternalStorage);
            }
            else if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.R) // API 30+
            {
                // Для Android 11+ нужно MANAGE_EXTERNAL_STORAGE для полного доступа
                permissions.Add(Manifest.Permission.ManageExternalStorage);
            }
            
            // Проверяем, есть ли уже все разрешения
            bool allGranted = true;
            
            foreach (var permission in permissions)
            {
                if (permission == Manifest.Permission.ManageExternalStorage)
                {
                    // Для MANAGE_EXTERNAL_STORAGE используем специальную проверку
                    if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.R)
                    {
                        if (!Android.OS.Environment.IsExternalStorageManager)
                        {
                            allGranted = false;
                            break;
                        }
                    }
                }
                else
                {
                    if (ContextCompat.CheckSelfPermission(activity, permission) != Permission.Granted)
                    {
                        allGranted = false;
                        break;
                    }
                }
            }
                
            if (allGranted)
                return Task.FromResult(true);

            tcs = new TaskCompletionSource<bool>();
            
            // Для Android 11+ с MANAGE_EXTERNAL_STORAGE нужен специальный подход
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.R && 
                permissions.Contains(Manifest.Permission.ManageExternalStorage))
            {
                // Убираем MANAGE_EXTERNAL_STORAGE из обычного запроса
                var regularPermissions = permissions.Where(p => p != Manifest.Permission.ManageExternalStorage).ToArray();
                
                if (regularPermissions.Length > 0)
                {
                    // Сначала запрашиваем обычные разрешения
                    ActivityCompat.RequestPermissions(activity, regularPermissions, 1001);
                }
                
                // Для MANAGE_EXTERNAL_STORAGE показываем сообщение пользователю
                Device.BeginInvokeOnMainThread(async () =>
                {
                    var result = await Application.Current.MainPage.DisplayAlert(
                        "Дополнительные разрешения",
                        "Для удаления файлов необходимо предоставить полный доступ к хранилищу в настройках.",
                        "Открыть настройки", "Отмена");
                    
                    if (result)
                    {
                        // Открываем настройки для MANAGE_EXTERNAL_STORAGE
                        var intent = new Android.Content.Intent(Android.Provider.Settings.ActionManageAppAllFilesAccessPermission);
                        intent.SetData(Android.Net.Uri.Parse("package:" + activity.PackageName));
                        activity.StartActivity(intent);
                    }
                    
                    tcs.TrySetResult(Android.OS.Environment.IsExternalStorageManager);
                });
            }
            else
            {
                // Обычный запрос разрешений
                ActivityCompat.RequestPermissions(activity, permissions.ToArray(), 1001);
            }
            
            // Подписка на результат разрешения
            ((MainActivity)activity).RequestPermissionsResultReceived = (requestCode, requestPermissions, grantResults) =>
            {
                if (requestCode == 1001)
                {
                    // Для основной функциональности достаточно хотя бы одного разрешения на чтение
                    bool hasStoragePermission = false;
                    bool hasWritePermission = false;
                    
                    for (int i = 0; i < requestPermissions.Length; i++)
                    {
                        if ((requestPermissions[i] == Manifest.Permission.ReadExternalStorage ||
                             requestPermissions[i] == Manifest.Permission.ReadMediaImages) &&
                            grantResults[i] == Permission.Granted)
                        {
                            hasStoragePermission = true;
                        }
                        
                        if (requestPermissions[i] == Manifest.Permission.WriteExternalStorage &&
                            grantResults[i] == Permission.Granted)
                        {
                            hasWritePermission = true;
                        }
                    }
                    
                    // Проверяем MANAGE_EXTERNAL_STORAGE если нужно
                    if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.R)
                    {
                        hasWritePermission = Android.OS.Environment.IsExternalStorageManager;
                    }
                    
                    tcs.TrySetResult(hasStoragePermission);
                }
            };
            return tcs.Task;
        }

        public bool CanRequestPermission()
        {
            var activity = Xamarin.Essentials.Platform.CurrentActivity;
            
            // Проверяем можем ли запросить разрешения на чтение
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Tiramisu)
            {
                return ActivityCompat.ShouldShowRequestPermissionRationale(activity, Manifest.Permission.ReadMediaImages) ||
                       ContextCompat.CheckSelfPermission(activity, Manifest.Permission.ReadMediaImages) != Permission.Granted;
            }
            else
            {
                return ActivityCompat.ShouldShowRequestPermissionRationale(activity, Manifest.Permission.ReadExternalStorage) ||
                       ContextCompat.CheckSelfPermission(activity, Manifest.Permission.ReadExternalStorage) != Permission.Granted;
            }
        }

        public void OpenAppSettings()
        {
            var activity = Xamarin.Essentials.Platform.CurrentActivity;
            var intent = new Android.Content.Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
            intent.SetData(Android.Net.Uri.Parse("package:" + activity.PackageName));
            activity.StartActivity(intent);
        }

        public Task<bool> CheckWritePermissionAsync()
        {
            var activity = Xamarin.Essentials.Platform.CurrentActivity;
            
            // Для Android 11+ (API 30+) проверяем MANAGE_EXTERNAL_STORAGE
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.R)
            {
                return Task.FromResult(Android.OS.Environment.IsExternalStorageManager);
            }
            // Для более старых версий проверяем WRITE_EXTERNAL_STORAGE
            else
            {
                var hasWritePermission = ContextCompat.CheckSelfPermission(activity, Manifest.Permission.WriteExternalStorage) == Permission.Granted;
                return Task.FromResult(hasWritePermission);
            }
        }
    }
}
