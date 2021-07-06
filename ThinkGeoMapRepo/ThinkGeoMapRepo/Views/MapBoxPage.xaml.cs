using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ThinkGeoMapRepo.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapBoxPage : ContentPage
    {
        public MapBoxPage()
        {
            InitializeComponent();

            mapView.Center = new Naxam.Mapbox.LatLng(21.028511, 105.804817);
            mapView.ZoomLevel = 15;
        }
    }
}