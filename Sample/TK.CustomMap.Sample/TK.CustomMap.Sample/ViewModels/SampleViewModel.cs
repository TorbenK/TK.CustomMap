using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using TK.CustomMap.Api;
using TK.CustomMap.Api.Google;
using TK.CustomMap.Api.OSM;
using TK.CustomMap.Overlays;
using TK.CustomMap.Utilities;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TK.CustomMap.Sample
{
    public class SampleViewModel : INotifyPropertyChanged
    {
        private TKRoute _selectedRoute;

        private MapSpan _mapRegion;
        private Position _mapCenter;
        private TKCustomMapPin _selectedPin;
        private ObservableCollection<TKCustomMapPin> _pins;
        private ObservableCollection<TKRoute> _routes;
        private ObservableCollection<TKCircle> _circles;
        private ObservableCollection<TKPolygon> _polygons;

        Dictionary<TKCustomMapPin, TKRoute> _pinRoutes = new Dictionary<TKCustomMapPin, TKRoute>();

        public MapSpan MapRegion
        {
            get { return this._mapRegion; }
            set
            {
                if (this._mapRegion != value)
                {
                    this._mapRegion = value;
                    this.OnPropertyChanged("MapRegion");
                }
            }
        }
        /// <summary>
        /// Polygons bound to the <see cref="TKCustomMap"/>
        /// </summary>
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
        /// <summary>
        /// Pins bound to the <see cref="TkCustomMap"/>
        /// </summary>
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
        /// <summary>
        /// Routes bound to the <see cref="TkCustomMap"/>
        /// </summary>
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
        /// <summary>
        /// Circles bound to the <see cref="TkCustomMap"/>
        /// </summary>
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
        /// <summary>
        /// Map center bound to the <see cref="TkCustomMap"/>
        /// </summary>
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
        /// <summary>
        /// Selected pin bound to the <see cref="TkCustomMap"/>
        /// </summary>
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
        /// <summary>
        /// Map Long Press bound to the <see cref="TkCustomMap"/>
        /// </summary>
        public Command<Position> MapLongPressCommand
        {
            get
            {
                return new Command<Position>(position => 
                {
                    var pin = new MyPin 
                    {
                        Position = position,
                        Title = string.Format("Pin {0}, {1}", position.Latitude, position.Longitude),
                        ShowCallout = true,
                        IsDraggable = true,
                        DefaultPinColor = Color.Aqua
                    };

                    this._pins.Add(pin);
                    
                });
            }
        }
        /// <summary>
        /// Map Clicked bound to the <see cref="TkCustomMap"/>
        /// </summary>
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
        /// <summary>
        /// Command when a place got selected
        /// </summary>
        public Command<IPlaceResult> PlaceSelectedCommand
        {
            get
            {
                return new Command<IPlaceResult>(async p =>
                {
                    if (Device.OS == TargetPlatform.Android)
                    {
                        var prediction = (TKNativeAndroidPlaceResult)p;

                        var service = DependencyService.Get<INativePlacesApi>();
                        service.Connect();
                        var details = await service.GetDetails(prediction.PlaceId);

                        this.MapCenter = details.Coordinate;
                    }
                    else if (Device.OS == TargetPlatform.iOS)
                    {
                        var prediction = (TKNativeiOSPlaceResult)p;

                        this.MapCenter = prediction.Details.Coordinate;
                    }
                });
            }
        }
        /// <summary>
        /// Pin Selected bound to the <see cref="TkCustomMap"/>
        /// </summary>
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
        /// <summary>
        /// Drag End bound to the <see cref="TkCustomMap"/>
        /// </summary>
        public Command<TKCustomMapPin> DragEndCommand
        {
            get 
            {
                return new Command<TKCustomMapPin>(async pin => 
                {
                    
                });
            }
        }
        /// <summary>
        /// Clear everything from the <see cref="TKCustomMap"/>
        /// </summary>
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
        /// <summary>
        /// Callout clicked bound to the <see cref="TkCustomMap"/>
        /// </summary>
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
        /// <summary>
        /// Opens the page to add a circle
        /// </summary>
        public Command AddCircleCommand
        {
            get
            {
                return new Command(() => 
                {
                    var addCirclePage = new AddCirclePage();
                    addCirclePage.AddCircle += (o, e) => 
                    {
                        this._circles.Add(
                            new TKCircle 
                            {
                                Color = e.Model.Color,
                                Center = this.MapCenter,
                                StrokeWidth = 0,
                                Radius = e.Model.Radius
                            });

                        Application.Current.MainPage.Navigation.PopModalAsync();
                    };
                    Application.Current.MainPage.Navigation.PushModalAsync(addCirclePage);

                    //this._circles.Add(new TKCircle 
                    //{
                    //    Color = Color.FromRgba(0, 150, 0, 80),
                    //    Center = this.MapCenter,
                    //    StrokeWidth = 0,
                    //    Radius = 1000
                    //});
                });
            }
        }
        public Command AddRouteCommand
        {
            get
            {
                return new Command(() => 
                {
                    var addRoutePage = new AddRoutePage();
                    Application.Current.MainPage.Navigation.PushModalAsync(addRoutePage);
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
