using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;
using Module40.ViewModels;

namespace Module40.Views
{
    public partial class ImageViewPage : ContentPage
    {
        public ImageViewPage(ImageInfo imageInfo)
        {
            InitializeComponent();
            BindingContext = new ImageViewViewModel(imageInfo);
        }
    }

    public class ImageViewViewModel : INotifyPropertyChanged
    {
        private readonly ImageInfo _imageInfo;
        
        public string ImagePath => _imageInfo.FilePath;
        public string FileName => _imageInfo.FileName;
        public string FormattedDate => _imageInfo.FormattedDate;
        
        public ICommand CloseCommand { get; }
        
        public event PropertyChangedEventHandler PropertyChanged;

        public ImageViewViewModel(ImageInfo imageInfo)
        {
            _imageInfo = imageInfo;
            CloseCommand = new Command(OnClose);
        }

        private async void OnClose()
        {
            await Application.Current.MainPage.Navigation.PopAsync();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
