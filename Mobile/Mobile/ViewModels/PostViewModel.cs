using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Xamarin.Essentials;
using Xamarin.Forms;

using Mobile.Views;
using System.Threading.Tasks;

namespace Mobile.ViewModels
{
    class PostViewModel : BaseViewModel
    {
        private string _imagePath;

        public string ImagePath
        {
            get
            {
                return _imagePath;
            }
            set
            {
                SetProperty(ref _imagePath, value);
                IsBusy = false;
            }
        }

        private double? _latitude;
        private double? _longitude;

        private string _location;

        public string Location
        {
            get
            {
                return _location;
            }
            set
            {
                SetProperty(ref _location, value + " \U000f01a4");
            }
        }

        public string Description { get; set; }

        public ICommand SetCurrentLocationCommand { get; set; }

        public ICommand EditLocationCommand { get; set; }

        public ICommand UploadCommand { get; set; }

        public PostViewModel()
        {
            IsBusy = true;
            Location = "Set location";

            SetCurrentLocationCommand = new Command(async () => await SetCurrentLocation());
            EditLocationCommand = new Command(async () => await SetLocation());
            UploadCommand = new Command(async () => await Upload());

            SetCurrentLocationCommand.Execute(null);
        }


        private async Task SetCurrentLocation()
        {
            try
            {
                GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.High);
                Location location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    await UpdateLocation(location.Latitude, location.Longitude);
                }
            }
            catch
            {
                return;
            }
        }

        private async Task SetLocation()
        {
            LocationPage locationPage = new LocationPage();
            LocationViewModel locationViewModel = new LocationViewModel(_latitude, _longitude);

            locationViewModel.PropertyChanged += async (sender, args) =>
            {

                if (!args.PropertyName.Equals("Position"))
                {
                    return;
                }

                await UpdateLocation(locationViewModel.Position.Latitude, locationViewModel.Position.Longitude);
            };

            locationPage.BindingContext = locationViewModel;

            await Application.Current.MainPage.Navigation.PushAsync(locationPage);
        }

        private async Task UpdateLocation(double latitude, double longitude)
        {
            string location = await GetLocationDetails(new Location(latitude, longitude));

            if (location == null)
            {
                return;
            }

            _latitude = latitude;
            _longitude = longitude;

            Location = location;
        }

        private async Task<string> GetLocationDetails(Location location)
        {
            IEnumerable<Placemark> placemarks = await Geocoding.GetPlacemarksAsync(location);
            Placemark placemark = placemarks.First();

            if (placemark != null && placemark.Locality != null && placemark.CountryName != null)
            {
                return $"{placemark.Locality}, {placemark.CountryName}";
            }

            return null;
        }

        private async Task Upload()
        {
            if (_latitude == null || _longitude == null)
            {
                await DisplayAlertAsync("Please set the location of this tree.");
            }

            try
            {
                IsBusy = true;

                await App.TreeService.PostTree(_imagePath, (double)_latitude, (double)_longitude, Description);
                await Application.Current.MainPage.Navigation.PopToRootAsync();
            }
            catch (Exception e)
            {
                await DisplayAlertAsync("Please verify that your device is connected, the photo doesn't contain adult content," +
                                        "the descritpion isn't offensive and that the location is valid.");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
