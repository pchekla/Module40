using System.ComponentModel;
using Module40.ViewModels;
using Xamarin.Forms;

namespace Module40.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}