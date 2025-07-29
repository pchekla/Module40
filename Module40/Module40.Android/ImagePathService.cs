using System.Collections.Generic;
using System.Linq;
using Module40.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(Module40.Droid.ImagePathService))]
namespace Module40.Droid
{
    public class ImagePathService : IImagePathService
    {
        public List<string> GetImagePaths()
        {
            var searchPaths = new List<string>
            {
                "/storage/emulated/0/Pictures",        // Основная папка Pictures
                "/storage/emulated/0/DCIM/Camera",     // Папка камеры
                "/storage/emulated/0/DCIM",            // Общая папка DCIM
                "/storage/emulated/0/Download",        // Папка загрузок
                Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures)?.AbsolutePath,
                Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDcim)?.AbsolutePath,
                Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)?.AbsolutePath
            };

            // Убираем null значения и дубликаты
            return searchPaths.Where(p => !string.IsNullOrEmpty(p)).Distinct().ToList();
        }
    }
}
