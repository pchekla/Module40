using Module40.ViewModels;

namespace Module40.Views
{
    public partial class ItemDetailPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}