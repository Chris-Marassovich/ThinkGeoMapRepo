using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkGeoMapRepo.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ThinkGeoMapRepo.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        MapPageViewModel _viewModel;

        public MapPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new MapPageViewModel(mapView);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.InitialiseMap();
        }
    }
}