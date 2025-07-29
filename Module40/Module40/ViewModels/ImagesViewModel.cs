using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Module40.ViewModels
{
    public class ImagesViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> Images { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public ImagesViewModel()
        {
            Images = new ObservableCollection<string>
            {
                // Пример ссылок на изображения
                "https://placekitten.com/200/200",
                "https://placekitten.com/201/200",
                "https://placekitten.com/202/200"
            };
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

