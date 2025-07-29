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
                
                // Для API 30+ добавляем MANAGE_EXTERNAL_STORAGE если нужен полный доступ
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.R)
                {
                    permissions.Add(Manifest.Permission.ManageExternalStorage);
                }
            }
            
            // Проверяем, есть ли уже все разрешения
            bool allGranted = permissions.All(permission => 
                ContextCompat.CheckSelfPermission(activity, permission) == Permission.Granted);
                
            if (allGranted)
                return Task.FromResult(true);

            tcs = new TaskCompletionSource<bool>();
            ActivityCompat.RequestPermissions(activity, permissions.ToArray(), 1001);
            
            // Подписка на результат разрешения
            ((MainActivity)activity).RequestPermissionsResultReceived = (requestCode, requestPermissions, grantResults) =>
            {
                if (requestCode == 1001)
                {
                    // Для основной функциональности достаточно хотя бы одного разрешения на чтение
                    bool hasStoragePermission = false;
                    
                    for (int i = 0; i < requestPermissions.Length; i++)
                    {
                        if ((requestPermissions[i] == Manifest.Permission.ReadExternalStorage ||
                             requestPermissions[i] == Manifest.Permission.ReadMediaImages ||
                             requestPermissions[i] == Manifest.Permission.ManageExternalStorage) &&
                            grantResults[i] == Permission.Granted)
                        {
                            hasStoragePermission = true;
                            break;
                        }
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
    }
}
