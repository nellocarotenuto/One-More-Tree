using Xamarin.Forms;
using Mobile.Views;
using Mobile.Models;
using Mobile.Services;
using System;
using Mobile.ViewModels;
using Xamarin.Essentials;

namespace Mobile
{
    public partial class App : Application
    { 
        // Service instances that can be injected
        public static DatabaseService Database { get; private set; }
        public static AuthService AuthService { get; private set; }
        public static TreeService TreeService { get; private set; }

        public App()
        {
            InitializeComponent();
            InitializeServices();

            MainPage mainPage = new MainPage();

            FeedPage feedPage = new FeedPage();
            feedPage.BindingContext = new FeedViewModel();
            mainPage.Children.Add(feedPage);

            MapPage mapPage = new MapPage();
            mapPage.BindingContext = new MapViewModel();
            mainPage.Children.Add(mapPage);

            MainPage = new NavigationPage(mainPage);
        }

        // Inizialize the services of the app
        private void InitializeServices()
        {
            Database = new DatabaseService();
            AuthService = new AuthService();
            TreeService = new TreeService();
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
