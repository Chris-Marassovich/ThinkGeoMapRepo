using System;
using ThinkGeoMapRepo.Services;
using ThinkGeoMapRepo.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: ExportFont("Font Awesome 5 Free-Regular-400.otf", Alias = "FA-R")]
[assembly: ExportFont("Font Awesome 5 Free-Solid-900.otf", Alias = "FA-S")]
[assembly: ExportFont("Font Awesome 5 Brands-Regular-400.otf", Alias = "FA-B")]

namespace ThinkGeoMapRepo
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
