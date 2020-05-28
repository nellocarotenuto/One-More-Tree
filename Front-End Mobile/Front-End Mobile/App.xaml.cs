﻿using Xamarin.Forms;
using Front_End_Mobile.Views;
using Front_End_Mobile.Models;
using Autofac;
using Front_End_Mobile.Services;

namespace Front_End_Mobile
{
    public partial class App : Application
    {
        // IContainer is provided by Autofac
        private static IContainer _container;

        // Service instances that can be injected
        public static DatabaseService Database { get { return _container.Resolve<DatabaseService>(); } }
        public static AuthService AuthService { get { return _container.Resolve<AuthService>(); } }
        public static TreeService TreeService { get { return _container.Resolve<TreeService>(); } }

        public App()
        {
            InitializeComponent();
            InitializeServices();
        }

        // Inizialize the services of the app
        private void InitializeServices()
        {
            ContainerBuilder builder = new ContainerBuilder();

            builder.RegisterType<AuthService>().SingleInstance();
            builder.RegisterType<DatabaseService>().SingleInstance();
            builder.RegisterType<TreeService>();
            
            _container = builder.Build();
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
