using System;
using System.Collections.Generic;
using System.ComponentModel;
using ThinkGeoMapRepo.Models;
using ThinkGeoMapRepo.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ThinkGeoMapRepo.Views
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