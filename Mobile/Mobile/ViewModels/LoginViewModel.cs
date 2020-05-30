using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Mobile.ViewModels
{
    class LoginViewModel : BaseViewModel
    {
        public bool isAuthenticated;

        public bool IsAuthenticated
        {
            get => isAuthenticated;
            set => SetProperty(ref isAuthenticated, value);
        }

        public ICommand FacebookCommand { get; }

        public LoginViewModel()
        {
            FacebookCommand = new Command(async () => await Authenticate("facebook"));
        }

        public async Task Authenticate(string provider)
        {
            try
            {
                await App.AuthService.Authenticate(provider);
                IsAuthenticated = true;
            }
            catch (TaskCanceledException)
            {
                await DisplayAlertAsync("Login is required to use the app.");
            }
            catch
            {
                await DisplayAlertAsync("Something went wrong while communicating with the server.");
            }
        }
    }

}
