using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Module40.Models;
using Module40.ViewModels;
using Module40.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Module40.Views
{
    public partial class ItemsPage
    {
        ItemsViewModel _viewModel;

        public ItemsPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new ItemsViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}