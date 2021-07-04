using System.ComponentModel;
using ThinkGeoMapRepo.ViewModels;
using Xamarin.Forms;

namespace ThinkGeoMapRepo.Views
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