using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Module40.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using System.Linq;
using System.Collections.Generic;

namespace Module40.ViewModels
{
    public class ImageInfo
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
    }

    public class ImagesViewModel : INotifyPropertyChanged
    {
        public const string PermissionDeniedAlert = "PermissionDeniedAlert";
        public ObservableCollection<ImageInfo> Images { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isLoading;
        public bool IsLoading 
        { 
            get => _isLoading; 
            set 
            { 
                _isLoading = value; 
                OnPropertyChanged(); 
            } 
        }

        private string _statusMessage = "";
        public string StatusMessage 
        { 
            get => _statusMessage; 
            set 
            { 
                _statusMessage = value; 
                OnPropertyChanged(); 
            } 
        }

        public bool PermissionDenied { get; set; }
        public Command RequestPermissionCommand { get; set; }
        public bool ShowOpenSettings { get; set; }
        public Command OpenSettingsCommand { get; set; }

        public ImagesViewModel()
        {
            Images = new ObservableCollection<ImageInfo>();
            RequestPermissionCommand = new Command(RequestPermission);
            OpenSettingsCommand = new Command(OpenSettings);
            RequestPermissionAndLoadImages();
        }

        private async void RequestPermissionAndLoadImages()
        {
            IsLoading = true;
            StatusMessage = "Проверка разрешений...";
            
            var permissionService = DependencyService.Get<IPermissionService>();
            var granted = await permissionService.CheckAndRequestStoragePermissionAsync();
            if (granted)
            {
                PermissionDenied = false;
                ShowOpenSettings = false;
                StatusMessage = "Загрузка изображений...";
                await LoadImagesAsync();
            }
            else
            {
                PermissionDenied = true;
                ShowOpenSettings = !permissionService.CanRequestPermission();
                StatusMessage = "Разрешение не получено";
                MessagingCenter.Send(this, PermissionDeniedAlert, "Для отображения изображений необходимо разрешение на чтение хранилища.");
            }
            
            IsLoading = false;
            OnPropertyChanged(nameof(PermissionDenied));
            OnPropertyChanged(nameof(ShowOpenSettings));
        }

        private void RequestPermission()
        {
            RequestPermissionAndLoadImages();
        }

        private void OpenSettings()
        {
            var permissionService = DependencyService.Get<IPermissionService>();
            permissionService.OpenAppSettings();
        }

        private async System.Threading.Tasks.Task LoadImagesAsync()
        {
            try
            {
                Images.Clear();
                var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp" };
                var foundImages = 0;

                // Получаем пути через платформенно-специфичный сервис
                var imagePathService = DependencyService.Get<IImagePathService>();
                var searchPaths = imagePathService?.GetImagePaths() ?? new List<string>();

                // Если сервис не найден, используем fallback
                if (searchPaths.Count == 0)
                {
                    searchPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
                }

                StatusMessage = $"Поиск в {searchPaths.Count} папках...";

                foreach (var path in searchPaths)
                {
                    try
                    {
                        if (!Directory.Exists(path))
                        {
                            StatusMessage = $"Путь не существует: {Path.GetFileName(path)}";
                            System.Diagnostics.Debug.WriteLine($"Путь не найден: {path}");
                            continue;
                        }

                        StatusMessage = $"Поиск в: {path}";
                        System.Diagnostics.Debug.WriteLine($"Поиск изображений в: {path}");
                        
                        var files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
                        System.Diagnostics.Debug.WriteLine($"Найдено файлов в {path}: {files.Length}");
                        
                        foreach (var file in files)
                        {
                            var extension = Path.GetExtension(file).ToLowerInvariant();
                            if (imageExtensions.Contains(extension))
                            {
                                var fileInfo = new FileInfo(file);
                                if (fileInfo.Length > 0) // Проверяем, что файл не пустой
                                {
                                    Images.Add(new ImageInfo
                                    {
                                        FilePath = file,
                                        FileName = Path.GetFileName(file)
                                    });
                                    foundImages++;
                                    System.Diagnostics.Debug.WriteLine($"Добавлено изображение: {file}");
                                }
                            }
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        StatusMessage = $"Нет доступа к: {Path.GetFileName(path)}";
                        System.Diagnostics.Debug.WriteLine($"Нет доступа к папке {path}: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        StatusMessage = $"Ошибка в: {Path.GetFileName(path)}";
                        System.Diagnostics.Debug.WriteLine($"Ошибка при обработке папки {path}: {ex.Message}");
                    }
                }

                StatusMessage = foundImages > 0 ? $"Найдено изображений: {foundImages}" : "Изображения не найдены в указанных папках";
                System.Diagnostics.Debug.WriteLine($"Всего найдено изображений: {foundImages}");
                
                if (foundImages == 0)
                {
                    // Добавляем тестовые изображения только если реальные не найдены
                    Images.Add(new ImageInfo 
                    { 
                        FilePath = "xamarin_logo.png", 
                        FileName = "Тестовое изображение 1 (нет реальных фото)" 
                    });
                    Images.Add(new ImageInfo 
                    { 
                        FilePath = "icon_about.png", 
                        FileName = "Тестовое изображение 2 (нет реальных фото)" 
                    });
                    StatusMessage = "Реальные изображения не найдены. Показаны тестовые изображения.";
                    System.Diagnostics.Debug.WriteLine("Реальные изображения не найдены, показаны тестовые");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Общая ошибка: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке изображений: {ex.Message}");
                
                // В случае ошибки показываем тестовые изображения
                Images.Add(new ImageInfo 
                { 
                    FilePath = "xamarin_logo.png", 
                    FileName = "Ошибка загрузки - тестовое изображение" 
                });
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
