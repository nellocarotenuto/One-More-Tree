using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Maps;

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
            }
        }

        public Map Map { get; private set; }

        public LocationViewModel(double? latitude, double? longitude)
        {
            MapSpan mapSpan;

            // Show the full map if position is not set, zoom in there otherwise
            if (latitude == null || longitude == null)
            {
                mapSpan = new MapSpan(new Position(42, 12.5), 14, 12);
            }
            else
            {
                Position = new Position((double)latitude, (double)longitude);
                mapSpan = new MapSpan(Position, 0.05, 0.05);
            }
            
            // Initialize the map
            Map = new Map(mapSpan)
            {
                MapType = MapType.Street
            };

            // Add a pin at the current position if set
            if (Position != null)
            {
                Map.Pins.Add(new Pin()
                {
                    Label = string.Empty,
                    Position = Position
                });
            }
            
            // Reset pin where the user clickes
            Map.MapClicked += async (sender, args) =>
            {
                Position = args.Position;

                Map.Pins.Clear();

                Map.Pins.Add(new Pin() {
                    Label = string.Empty,
                    Position = Position
                });
            };
        }

    }
}
