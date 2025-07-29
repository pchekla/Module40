using Module40.Models;
using Module40.ViewModels;
using Xamarin.Forms;

namespace Module40.Views
{
    public partial class NewItemPage
    {
        public Item Item { get; set; }

        public NewItemPage()
        {
            InitializeComponent();
            BindingContext = new NewItemViewModel();
        }
    }
}