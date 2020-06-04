using Mobile.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Mobile.ViewModels
{
    class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            ShowAboutCommand = new Command(async () => await ShowAbout());
        }

        public ICommand ShowAboutCommand { get; set; }

        public async Task ShowAbout()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new AboutPage());
        }
    }

}
