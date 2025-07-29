using System;
using System.Collections.Generic;
using System.ComponentModel;
using Module40.Models;
using Module40.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Module40.Views
{
    public partial class NewItemPage : ContentPage
    {
        public Item Item { get; set; }

        public NewItemPage()
        {
            InitializeComponent();
            BindingContext = new NewItemViewModel();
        }
    }
}