using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TK.CustomMap.Api;
using TK.CustomMap.Api.Google;
using TK.CustomMap.Api.OSM;
using TK.CustomMap.Overlays;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TK.CustomMap.Sample
{
    public class SampleViewModel : INotifyPropertyChanged
    {
        private Position _mapCenter;
        private TKCustomMapPin _selectedPin;
        private ObservableCollection<TKCustomMapPin> _pins;
        private ObservableCollection<TKRoute> _routes;

        Dictionary<TKCustomMapPin, TKRoute> _pinRoutes = new Dictionary<TKCustomMapPin, TKRoute>();

        public ObservableCollection<TKCustomMapPin> Pins
        {
            get { return this._pins; }
            set 
            {
                if (this._pins != value)
                {
                    this._pins = value;
                    this.OnPropertyChanged("Pins");
                }
            }
        }
        public ObservableCollection<TKRoute> Routes
        {
            get { return this._routes; }
            set
            {
                if (this._routes != value)
                {
                    this._routes = value;
                    this.OnPropertyChanged("Routes");
                }
            }
        }
        public Position MapCenter
        {
            get { return this._mapCenter; }
            set 
            {
                if (this._mapCenter != value)
                {
                    this._mapCenter = value;
                    this.OnPropertyChanged("MapCenter");
                }
            }
        }
        public TKCustomMapPin SelectedPin
        {
            get { return this._selectedPin; }
            set
            {
                if (this._selectedPin != value)
                {
                    this._selectedPin = value;
                    this.OnPropertyChanged("SelectedPin");
                }
            }
        }

        public Command<Position> MapLongPressCommand
        {
            get
            {
                return new Command<Position>(async position => 
                {
                    var pin = new TKCustomMapPin 
                    {
                        Position = position,
                        Title = string.Format("Pin {0}, {1}", position.Latitude, position.Longitude),
                        ShowCallout = true,
                        IsDraggable = true
                    };

                    this._pins.Add(pin);
                    this.MapCenter = position;



                    if (this._routes == null)
                    {
                        this.Routes = new ObservableCollection<TKRoute>();
                    }

                    var routeResult = await GmsDirection.Instance.CalculateRoute(
                        this._pins.First().Position, 
                        pin.Position, 
                        GmsDirectionTravelMode.Driving, 
                        "de");

                    if (routeResult != null && routeResult.Status == GmsDirectionResultStatus.Ok)
                    {
                        var route = new TKRoute
                        {
                            LineWidth = 3,
                            LineColor = Color.Blue,
                            RouteCoordinates = new List<Position>(routeResult.Routes.First().Polyline.Positions)
                        };
                        this._routes.Add(route);

                        this._pinRoutes.Add(pin, route);
                    }
                });
            }
        }
        public Command<Position> MapClickedCommand
        {
            get
            {
                return new Command<Position>((positon) => 
                {
                    this.SelectedPin = null;
                });
            }
        }
        public Command<IPlaceResult> PlaceSelectedCommand
        {
            get
            {
                return new Command<IPlaceResult>(async p =>
                {
                    var googlePlace = p as GmsPlacePrediction;

                    if (googlePlace != null)
                    {
                        var details = await GmsPlace.Instance.GetDetails(p.PlaceId);

                        if (details.Status == GmsDetailsResultStatus.Ok)
                            this.MapCenter = details.Item.Geometry.Location.ToPosition();

                        return;
                    }

                    var osmPlace = p as OsmNominatimResult;
                    if (osmPlace != null)
                    {
                        this.MapCenter = new Position(osmPlace.Latitude, osmPlace.Longitude);
                    }
                });
            }
        }

        public Command PinSelectedCommand
        {
            get
            {
                return new Command(() =>
                {
                    this.MapCenter = this.SelectedPin.Position;
                });
            }
        }
        public Command<TKCustomMapPin> DragEndCommand
        {
            get 
            {
                return new Command<TKCustomMapPin>(async pin => 
                {
                    if (this._routes == null) return;

                    var route = this._pinRoutes[pin];

                    var routeResult = await GmsDirection.Instance.CalculateRoute(
                       this._pins.First().Position,
                       pin.Position,
                       GmsDirectionTravelMode.Driving,
                       "de");

                    if (routeResult != null)
                    {
                        route.RouteCoordinates = new List<Position>(routeResult.Routes.First().Polyline.Positions);
                    }
                });
            }
        }

        public SampleViewModel()
        {
            this._mapCenter = new Position(40.7142700, -74.0059700);
            this._pins = new ObservableCollection<TKCustomMapPin>(new TKCustomMapPin[] 
            {
                new TKCustomMapPin
                {
                    Position = new Position(40.7142700, -74.0059700),
                    ShowCallout = false,
                    Image = "https://maps.gstatic.com/mapfiles/ms2/micons/purple.png",
                    IsDraggable = false,
                    Title = "New York"
                }
            });
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var ev = this.PropertyChanged;

            if (ev != null)
                ev(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
