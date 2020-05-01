using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Front_End_Mobile.Services;
using Front_End_Mobile.Views;

namespace Front_End_Mobile
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new MainPage();
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
