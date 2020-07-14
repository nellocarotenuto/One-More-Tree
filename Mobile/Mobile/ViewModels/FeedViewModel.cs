using Mobile.Models;
using Mobile.Views;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Mobile.ViewModels
{
    class FeedViewModel : BaseViewModel
    {
        public ObservableCollection<Tree> Trees { get; private set; }

        private bool _isRefreshing;

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }
        
        public ICommand RefreshCommand { get; }
        public ICommand PostCommand { get; }

        public FeedViewModel()
        {
            Trees = new ObservableCollection<Tree>();

            RefreshCommand = new Command(async () => await RefreshFeed());
            PostCommand = new Command<string>(async (string mode) => await Post(mode));
        }

        public override async void OnAppearing()
        {
            base.OnAppearing();

            await RefreshFeed();
        }

        public async Task RefreshFeed()
        {
            NetworkAccess networkAccess = Connectivity.NetworkAccess;
            if (networkAccess == NetworkAccess.Internet)
            {
                try
                {
                    await App.TreeService.Synchronize();
                }
                catch
                {
                    await DisplayAlertAsync("Something went wrong while communicating with the server.");
                }
            }
            else
            {
                await DisplayAlertAsync("No Internet access, unable to refresh feed.");
            }

            Trees.Clear();

            List<Tree> trees = await App.Database.GetTreesAsync();
            trees.Reverse();

            foreach (Tree tree in trees)
            {
                Trees.Add(tree);
            }

            IsRefreshing = false;
        }

        public async Task Post(string mode)
        {
            if (IsBusy)
            {
                return;
            }
            
            IsBusy = true;

            if (!App.AuthService.IsAuthenticated())
            {
                // First time so prompt for login
                LoginPage loginPage = new LoginPage();
                LoginViewModel loginViewModel = new LoginViewModel();

                loginPage.Disappearing += (sender, args) =>
                {
                    IsBusy = false;
                };

                loginViewModel.PropertyChanged += async (sender, args) =>
                {
                    if (!args.PropertyName.Equals("IsAuthenticated"))
                    {
                        return;
                    }

                    if (loginViewModel.IsAuthenticated)
                    {
                        await Application.Current.MainPage.Navigation.PopAsync();
                        await GetMedia(mode);
                    }
                };

                loginPage.BindingContext = loginViewModel;
                await Application.Current.MainPage.Navigation.PushAsync(loginPage);
            }
            else
            {
                await GetMedia(mode);
                IsBusy = false;
            }
        }

        private async Task GetMedia(string mode)
        {
            PostPage postPage = new PostPage();
            PostViewModel postViewModel = new PostViewModel();
            postPage.BindingContext = postViewModel;

            await Application.Current.MainPage.Navigation.PushAsync(postPage);

            MediaFile photo = null;

            try
            {
                if ("Camera".Equals(mode))
                {
                    if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                    {
                        await Application.Current.MainPage.Navigation.PopAsync();
                        await DisplayAlertAsync("No camera detected.");
                        return;
                    }

                    photo = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                    {
                        CompressionQuality = 85,
                        DefaultCamera = CameraDevice.Front
                    });
                }
                else if ("Gallery".Equals(mode))
                {
                    await CrossMedia.Current.Initialize();

                    if (!CrossMedia.Current.IsPickPhotoSupported)
                    {
                        await Application.Current.MainPage.Navigation.PopAsync();
                        await DisplayAlertAsync("Photos not supported.");
                        return;
                    }

                    photo = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                    {
                        CompressionQuality = 85,
                    });
                }

                if (photo == null)
                {
                    await Application.Current.MainPage.Navigation.PopAsync();
                    return;
                }

                postViewModel.ImagePath = photo.Path;
            }
            catch (MediaPermissionException)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
                await DisplayAlertAsync("Please allow camera and storage permissions in app settings to upload a new post.");
                return;
            }
        }
    }
}
