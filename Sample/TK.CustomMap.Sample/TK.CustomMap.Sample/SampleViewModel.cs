using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
        private ObservableCollection<TKCircle> _circles;
        private ObservableCollection<TKPolygon> _polygons;

        Dictionary<TKCustomMapPin, TKRoute> _pinRoutes = new Dictionary<TKCustomMapPin, TKRoute>();

        public ObservableCollection<TKPolygon> Polygons
        {
            get { return this._polygons; }
            set
            {
                if (this._polygons != value)
                {
                    this._polygons = value;
                    this.OnPropertyChanged("Polygons");
                }
            }
        }
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
        public ObservableCollection<TKCircle> Circles
        {
            get { return this._circles; }
            set
            {
                if (this._circles != value)
                {
                    this._circles = value;
                    this.OnPropertyChanged("Circles");
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

                    if (this._pins.Count == 4)
                    {
                        this._polygons.Add(new TKPolygon 
                        {
                            Coordinates = this._pins.Select(i => i.Position).ToList(),
                            Color = Color.FromRgba(0, 0, 120, 80),
                            StrokeColor = Color.Navy,
                            StrokeWidth = 0.2f
                        });
                    }

                    this.MapCenter = position;


                    if (this._pins.Count == 1) return;

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
                            Color = Color.Blue,
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
                    var myPin = (MyPin)pin;

                    if (this._routes == null) return;
                    if (!this._pinRoutes.ContainsKey(myPin)) return;

                    var route = this._pinRoutes[myPin];

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
        public Command ClearEverythingCommand
        {
            get
            {
                return new Command(() => 
                {
                    this.Pins.Clear();
                    if(this.Routes != null)
                        this.Routes.Clear();
                    this.Circles.Clear();
                    this.Polygons.Clear();
                });
            }
        }
        public Command CalloutClickedCommand
        {
            get
            {
                return new Command(() => 
                {
                    Application.Current.MainPage.DisplayAlert(
                        "Callout Clicked",
                        string.Format("Callout of pin {0} clicked", this.SelectedPin.Title),
                        "Cool story bro");
                });
            }
        }
        public Command AddCircleCommand
        {
            get
            {
                return new Command(() => 
                {
                    this._circles.Add(new TKCircle 
                    {
                        Color = Color.FromRgba(0, 150, 0, 80),
                        Center = this.MapCenter,
                        StrokeWidth = 0,
                        Radius = 1000
                    });
                });
            }
        }

        public SampleViewModel()
        {
            this._mapCenter = new Position(40.7142700, -74.0059700);
            this._pins = new ObservableCollection<TKCustomMapPin>(new MyPin[] 
            {
                new MyPin
                {
                    Position = new Position(40.7142700, -74.0059700),
                    ShowCallout = false,
                    Image = "https://maps.gstatic.com/mapfiles/ms2/micons/purple.png",
                    IsDraggable = false,
                    Title = "New York"
                }
            });
            this._polygons = new ObservableCollection<TKPolygon>();
            this._circles = new ObservableCollection<TKCircle>(new TKCircle[] 
            {
                new TKCircle
                {
                    Center = new Position(40.7142700, -74.0059700),
                    Color = Color.FromRgba(99, 0, 0, 80),
                    Radius = 1000
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
