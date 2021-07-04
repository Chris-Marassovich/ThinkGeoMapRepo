using System;
using ThinkGeoMapRepo.Services;
using ThinkGeoMapRepo.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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
