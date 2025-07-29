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
    public class ImageInfo : INotifyPropertyChanged
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        
        private bool _isSelected;
        public bool IsSelected 
        { 
            get => _isSelected; 
            set 
            { 
                _isSelected = value; 
                OnPropertyChanged(); 
            } 
        }

        public DateTime CreatedDate 
        { 
            get 
            { 
                try 
                { 
                    return File.GetCreationTime(FilePath); 
                } 
                catch 
                { 
                    return DateTime.Now; 
                } 
            } 
        }

        public string FormattedDate => CreatedDate.ToString("dd.MM.yyyy HH:mm");

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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

        private bool _hasSelectedImages;
        public bool HasSelectedImages 
        { 
            get => _hasSelectedImages; 
            set 
            { 
                _hasSelectedImages = value; 
                OnPropertyChanged(); 
            } 
        }

        public bool PermissionDenied { get; set; }
        public Command RequestPermissionCommand { get; set; }
        public bool ShowOpenSettings { get; set; }
        public Command OpenSettingsCommand { get; set; }
        public Command<ImageInfo> SelectImageCommand { get; set; }
        public Command OpenSelectedImageCommand { get; set; }
        public Command DeleteSelectedImagesCommand { get; set; }

        public ImagesViewModel()
        {
            Images = new ObservableCollection<ImageInfo>();
            RequestPermissionCommand = new Command(RequestPermission);
            OpenSettingsCommand = new Command(OpenSettings);
            SelectImageCommand = new Command<ImageInfo>(OnSelectImage);
            OpenSelectedImageCommand = new Command(OnOpenSelectedImage, CanOpenSelectedImage);
            DeleteSelectedImagesCommand = new Command(OnDeleteSelectedImages, CanDeleteSelectedImages);
            RequestPermissionAndLoadImages();
        }

        private void OnSelectImage(ImageInfo imageInfo)
        {
            if (imageInfo == null) return;

            // Переключаем выделение текущего изображения (множественный выбор)
            imageInfo.IsSelected = !imageInfo.IsSelected;
            
            UpdateSelectionState();
        }

        private void UpdateSelectionState()
        {
            var selectedImages = Images.Where(i => i.IsSelected).ToList();
            var selectedCount = selectedImages.Count;
            
            HasSelectedImages = selectedCount > 0;
            
            // Кнопка "Открыть" активна только при выборе одного изображения
            var canOpen = selectedCount == 1;
            // Кнопка "Удалить" активна при выборе одного или нескольких изображений
            var canDelete = selectedCount > 0;
            
            ((Command)OpenSelectedImageCommand).ChangeCanExecute();
            ((Command)DeleteSelectedImagesCommand).ChangeCanExecute();
        }

        private bool CanOpenSelectedImage()
        {
            return Images.Count(i => i.IsSelected) == 1;
        }

        private bool CanDeleteSelectedImages()
        {
            return Images.Any(i => i.IsSelected);
        }

        private async void OnOpenSelectedImage()
        {
            var selectedImage = Images.FirstOrDefault(i => i.IsSelected);
            if (selectedImage != null)
            {
                // Переходим на страницу просмотра изображения
                await Application.Current.MainPage.Navigation.PushAsync(
                    new Views.ImageViewPage(selectedImage));
            }
        }

        private async void OnDeleteSelectedImages()
        {
            var selectedImages = Images.Where(i => i.IsSelected).ToList();
            if (!selectedImages.Any()) return;

            // Сначала проверяем разрешения на запись
            var permissionService = DependencyService.Get<IPermissionService>();
            var hasWritePermission = await permissionService.CheckWritePermissionAsync();
            
            if (!hasWritePermission)
            {
                var requestResult = await Application.Current.MainPage.DisplayAlert(
                    "Нужны разрешения",
                    "Для удаления файлов необходимо разрешение на запись. Предоставить разрешение?",
                    "Да", "Отмена");

                if (requestResult)
                {
                    // Запрашиваем разрешения заново
                    await permissionService.CheckAndRequestStoragePermissionAsync();
                    
                    // Проверяем еще раз
                    hasWritePermission = await permissionService.CheckWritePermissionAsync();
                    
                    if (!hasWritePermission)
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Ошибка",
                            "Без разрешения на запись удаление файлов невозможно. Откройте настройки приложения и предоставьте необходимые разрешения.",
                            "OK");
                        return;
                    }
                }
                else
                {
                    return; // Пользователь отказался предоставлять разрешения
                }
            }

            var imagesToDelete = selectedImages.Count;
            
            // Формируем сообщение с списком файлов для удаления
            string confirmationMessage;
            if (imagesToDelete == 1)
            {
                confirmationMessage = $"Вы действительно хотите удалить изображение?\n\n• {selectedImages[0].FileName}\n\nФайл будет удален навсегда.";
            }
            else
            {
                var filesList = string.Join("\n", selectedImages.Take(5).Select(img => $"• {img.FileName}"));
                if (imagesToDelete > 5)
                {
                    filesList += $"\n• ... и еще {imagesToDelete - 5} файлов";
                }
                
                confirmationMessage = $"Вы действительно хотите удалить {imagesToDelete} изображений?\n\n{filesList}\n\nФайлы будут удалены навсегда.";
            }
            
            var result = await Application.Current.MainPage.DisplayAlert(
                "Подтверждение удаления",
                confirmationMessage,
                "Удалить", "Отмена");

            if (result)
            {
                var deletedCount = 0;
                var failedCount = 0;
                var failedFiles = new List<string>();

                foreach (var image in selectedImages)
                {
                    try
                    {
                        // Удаляем файл из файловой системы
                        if (File.Exists(image.FilePath))
                        {
                            File.Delete(image.FilePath);
                            System.Diagnostics.Debug.WriteLine($"Успешно удален файл: {image.FilePath}");
                            
                            // Проверяем, действительно ли файл удален
                            if (!File.Exists(image.FilePath))
                            {
                                // Удаляем из коллекции только если файл действительно удален
                                Images.Remove(image);
                                deletedCount++;
                            }
                            else
                            {
                                failedCount++;
                                failedFiles.Add(image.FileName);
                                System.Diagnostics.Debug.WriteLine($"Файл не был удален: {image.FilePath}");
                            }
                        }
                        else
                        {
                            // Файл не существует, удаляем из коллекции
                            Images.Remove(image);
                            deletedCount++;
                            System.Diagnostics.Debug.WriteLine($"Файл не найден, удален из списка: {image.FilePath}");
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        failedCount++;
                        failedFiles.Add(image.FileName);
                        System.Diagnostics.Debug.WriteLine($"Нет прав на удаление файла {image.FilePath}: {ex.Message}");
                    }
                    catch (IOException ex)
                    {
                        failedCount++;
                        failedFiles.Add(image.FileName);
                        System.Diagnostics.Debug.WriteLine($"Ошибка ввода-вывода при удалении {image.FilePath}: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        failedFiles.Add(image.FileName);
                        System.Diagnostics.Debug.WriteLine($"Неожиданная ошибка при удалении {image.FilePath}: {ex.Message}");
                    }
                }

                UpdateSelectionState();
                
                // Показываем результат операции
                if (failedCount == 0)
                {
                    StatusMessage = $"Успешно удалено файлов: {deletedCount}";
                }
                else if (deletedCount > 0)
                {
                    StatusMessage = $"Удалено: {deletedCount}, не удалось: {failedCount}";
                    await Application.Current.MainPage.DisplayAlert(
                        "Частичное удаление",
                        $"Удалено файлов: {deletedCount}\nНе удалось удалить: {failedCount}\n\nПроблемные файлы:\n{string.Join("\n", failedFiles.Take(5))}{(failedFiles.Count > 5 ? "\n..." : "")}",
                        "OK");
                }
                else
                {
                    StatusMessage = $"Не удалось удалить ни одного файла";
                    await Application.Current.MainPage.DisplayAlert(
                        "Ошибка удаления",
                        $"Не удалось удалить ни одного файла.\n\nВозможные причины:\n- Нет разрешений на запись\n- Файлы заблокированы другим приложением\n- Файлы находятся в системной папке",
                        "OK");
                }
            }
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

                StatusMessage = foundImages > 0 ? $"Найдено изображений: {foundImages}" : "Изображения не найдены";
                System.Diagnostics.Debug.WriteLine($"Всего найдено изображений: {foundImages}");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Общая ошибка: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке изображений: {ex.Message}");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
