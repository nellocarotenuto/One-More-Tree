using Mobile.Models;
using Mobile.Views;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Mobile.ViewModels
{
    class FeedViewModel : BaseViewModel
    {
        public bool isRefreshing;

        public bool IsRefreshing
        {
            get => isRefreshing;
            set => SetProperty(ref isRefreshing, value);
        }
        
        public ICommand RefreshCommand { get; }
        public ICommand PostCommand { get; }

        public ObservableCollection<Tree> Trees { get; private set; }

        public FeedViewModel()
        {
            Trees = new ObservableCollection<Tree>();

            RefreshCommand = new Command(async () => await RefreshFeed());
            RefreshCommand.Execute(null);

            PostCommand = new Command<string>(async (string mode) =>
            {
                if (!App.AuthService.IsAuthenticated())
                {
                    // First time so prompt for login
                    LoginPage loginPage = new LoginPage();
                    LoginViewModel loginViewModel = new LoginViewModel();

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
                }
            });
        }

        public async Task RefreshFeed()
        {
            try
            {
                IsRefreshing = true;
                await App.TreeService.Synchronize();

                Trees.Clear();

                foreach (Tree tree in await App.Database.GetTreesAsync())
                {
                    Trees.Add(tree);
                }
            }
            catch
            {
                await DisplayAlertAsync("Something went wrong while communicating with the server.");
            }
            finally
            {
                IsRefreshing = false;
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
                        await DisplayAlertAsync("No camera detected.");
                        return;
                    }

                    photo = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                    {
                        CompressionQuality = 92,
                        DefaultCamera = CameraDevice.Front
                    });
                }
                else if ("Gallery".Equals(mode))
                {
                    await CrossMedia.Current.Initialize();

                    if (!CrossMedia.Current.IsPickPhotoSupported)
                    {
                        await DisplayAlertAsync("Photos not supported.");
                        return;
                    }

                    photo = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                    {
                        CompressionQuality = 92,
                    });
                }
            }
            catch (MediaPermissionException)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
                await DisplayAlertAsync("Please allow camera and storage permissions in app settings to upload a new post.");
                return;
            }
            
            if (photo == null)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
                return;
            }

            postViewModel.ImagePath = photo.Path;
        }
    }
}
