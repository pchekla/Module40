using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Module40.ViewModels
{
    public class PinViewModel : INotifyPropertyChanged
    {
        private const string PinKey = "UserPin";
        private string _pin;
        private string _message;
        private string _placeholder;
        private Color _messageColor = Color.Black;
        public event PropertyChangedEventHandler PropertyChanged;

        public string Pin
        {
            get => _pin;
            set { _pin = value; OnPropertyChanged(); }
        }

        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        }

        public string Placeholder
        {
            get => _placeholder;
            set { _placeholder = value; OnPropertyChanged(); }
        }

        public Color MessageColor
        {
            get => _messageColor;
            set { _messageColor = value; OnPropertyChanged(); }
        }

        public ICommand SubmitCommand { get; }

        private bool IsPinSet => Preferences.ContainsKey(PinKey);

        public PinViewModel()
        {
            SubmitCommand = new Command(OnSubmit);
            RefreshState();
        }

        public void RefreshState()
        {
            if (IsPinSet)
            {
                Message = "Введите PIN-код";
                Placeholder = "Введите PIN-код";
            }
            else
            {
                Message = "Установите PIN-код";
                Placeholder = "Установите PIN-код";
            }
            
            // Сбрасываем цвет на черный при обновлении состояния
            MessageColor = Color.Black;
            
            // Очищаем поле ввода
            Pin = string.Empty;
        }

        private async void OnSubmit()
        {
            if (!IsPinSet)
            {
                if (!string.IsNullOrEmpty(Pin) && Pin.Length == 4)
                {
                    Preferences.Set(PinKey, Pin);
                    await Application.Current.MainPage.Navigation.PushAsync(new Views.ImagesPage());
                }
                else
                {
                    Message = "PIN-код должен содержать 4 цифры";
                    MessageColor = Color.Red;
                }
            }
            else
            {
                var savedPin = Preferences.Get(PinKey, string.Empty);
                if (Pin == savedPin)
                {
                    await Application.Current.MainPage.Navigation.PushAsync(new Views.ImagesPage());
                }
                else
                {
                    Message = "Неверный PIN-код";
                    MessageColor = Color.Red;
                }
            }
            Pin = string.Empty;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
