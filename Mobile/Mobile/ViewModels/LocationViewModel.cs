using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Essentials = Xamarin.Essentials;

namespace Mobile.ViewModels
{
    class LocationViewModel : BaseViewModel
    {

        private Position _position;

        public Position Position
        {
            get
            {
                return _position;
            }
            set
            {
                SetProperty(ref _position, value);

                Map.Pins.Clear();

                Map.Pins.Add(new Pin()
                {
                    Label = string.Empty,
                    Position = _position
                });

                Map.MoveToRegion(new MapSpan(_position, 0.05, 0.05));
            }
        }

        public Map Map { get; private set; }

        public ICommand SetCurrentLocationCommand { get; set; }

        public LocationViewModel(double? latitude, double? longitude)
        {
            // Initialize the map
            Map = new Map()
            {
                MapType = MapType.Street
            };

            // Show the full map if position is not set, zoom in there otherwise
            if (latitude == null || longitude == null)
            {
                Map.MoveToRegion(new MapSpan(new Position(42, 12.5), 14, 12));
            }
            else
            {
                Position = new Position((double)latitude, (double)longitude);
                Map.MoveToRegion(new MapSpan(Position, 0.05, 0.05));
            }
            
            // Reset pin where the user clickes
            Map.MapClicked += (sender, args) => Position = args.Position;

            SetCurrentLocationCommand = new Command(async () => await SetCurrentLocation());
        }

        public async Task SetCurrentLocation()
        {
            try
            {
                Essentials.GeolocationRequest request = new Essentials.GeolocationRequest(Essentials.GeolocationAccuracy.High);
                Essentials.Location location = await Essentials.Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    Position = new Position(location.Latitude, location.Longitude);
                }
            }
            catch
            {
                await DisplayAlertAsync("Unable to get current location. Ensure geolocation is active on the device.");
            }
        }

    }
}
