using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System;
using System.Linq;
using TK.CustomMap.Api;
using TK.CustomMap.Api.Google;
using TK.CustomMap.Api.OSM;
using TK.CustomMap.Overlays;
using TK.CustomMap.Utilities;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using TK.CustomMap.Sample.Pages;
using System.Threading.Tasks;
using TK.CustomMap.Interfaces;

namespace TK.CustomMap.Sample
{
    public class SampleViewModel : INotifyPropertyChanged
    {
        private TKTileUrlOptions _tileUrlOptions;

        private MapSpan _mapRegion;
        private Position _mapCenter;
        private TKCustomMapPin _selectedPin;
        private ObservableCollection<TKCustomMapPin> _pins;
        private ObservableCollection<TKRoute> _routes;
        private ObservableCollection<TKCircle> _circles;
        private ObservableCollection<TKPolyline> _lines;
        private ObservableCollection<TKPolygon> _polygons;

        public TKTileUrlOptions TilesUrlOptions
        {
            get 
            {
                return this._tileUrlOptions;
                //return new TKTileUrlOptions(
                //    "http://a.basemaps.cartocdn.com/dark_all/{2}/{0}/{1}.png", 256, 256, 0, 18);
                //return new TKTileUrlOptions(
                //    "http://a.tile.openstreetmap.org/{2}/{0}/{1}.png", 256, 256, 0, 18);
            }
            set
            {
                if(this._tileUrlOptions != value)
                {
                    this._tileUrlOptions = value;
                    this.OnPropertyChanged("TilesUrlOptions");
                }
            }
        }
        public IRendererFunctions MapFunctions { get; set; }
        public Command RunSimulationCommand
        {
            get
            {
                return new Command(async _=>
                {
                    if (!(await Application.Current.MainPage.DisplayAlert("Start Test?", "Start simulation test?", "Yes", "No")))
                        return;

                    #region PinTest

                    var pin = new TKCustomMapPin 
                    {
                        Position = new Position(40.718577, -74.083754)
                    };

                    this._pins.Add(pin);
                    await Task.Delay(1000);

                    pin.DefaultPinColor = Color.Purple;
                    await Task.Delay(1000);
                    pin.DefaultPinColor = Color.Green;
                    await Task.Delay(1000);

                    pin.Image = Device.OnPlatform("Icon-Small.png", "icon.png", string.Empty);
                    await Task.Delay(1000);
                    pin.Image = null;
                    await Task.Delay(1000);

                    this._pins.Remove(pin);
                    await Task.Delay(1000);
                    this._pins.Add(pin);
                    await Task.Delay(1000);
                    pin.Position = new Position(40.718281, -74.085179);
                    await Task.Delay(1000);
                    pin.Position = new Position(40.717476, -74.080915);
                    await Task.Delay(1000);
                    pin.Position = new Position(40.718577, -74.083754);
                    await Task.Delay(1000);
                    this._pins.Clear();

                    #endregion

                    #region Circles Test

                    var circle = new TKCircle 
                    {
                        Center = new Position(40.659743, -74.049422),
                        Color = Color.Red,
                        Radius = 1000
                    };
                    this._circles.Add(circle);
                    await Task.Delay(1000);

                    circle.Color = Color.Green;
                    await Task.Delay(1000);
                    circle.Color = Color.Purple;
                    await Task.Delay(1000);

                    circle.Radius = 2000;
                    await Task.Delay(1000);
                    circle.Radius = 3000;
                    await Task.Delay(1000);

                    circle.Center = new Position(40.718577, -74.083754);
                    await Task.Delay(1000);

                    this._circles.Remove(circle);
                    await Task.Delay(1000);
                    this._circles.Add(circle);
                    await Task.Delay(1000);
                    this._circles.Clear();

                    #endregion

                    #region Lines Test

                    this.Lines = new ObservableCollection<TKPolyline>();

                    var line = new TKPolyline 
                    {
                        Color = Color.Pink,
                        LineWidth = 2f,
                        LineCoordinates = new List<Position>(new Position[] 
                        {
                            new Position(40.647241, -74.081007),
                            new Position(40.702873, -74.016162)
                        })
                    };

                    this._lines.Add(line);
                    await Task.Delay(1000);

                    line.Color = Color.Red;
                    await Task.Delay(1000);
                    line.Color = Color.Green;
                    await Task.Delay(1000);

                    line.LineCoordinates =  new List<Position>(new Position[] 
                    {
                        new Position(40.647241, -74.081007),
                        new Position(40.702873, -74.016162),
                        new Position(40.690602, -74.017309)
                    });
                    await Task.Delay(1000);
                    this._lines.Remove(line);
                    await Task.Delay(1000);
                    this._lines.Add(line);
                    await Task.Delay(1000);
                    this._lines.Clear();

                    #endregion

                    #region Polygon Test

                    this.Polygons = new ObservableCollection<TKPolygon>();

                    var poly = new TKPolygon 
                    {
                        StrokeColor = Color.Green,
                        StrokeWidth = 2f,
                        Color = Color.Red,
                        Coordinates = new List<Position>(new Position[] 
                        {
                            new Position(40.716901, -74.055969),
                            new Position(40.699878, -73.986296),
                            new Position(40.636811, -74.076240)
                        })
                    };

                    this._polygons.Add(poly);
                    await Task.Delay(1000);

                    poly.StrokeColor = Color.Purple;
                    await Task.Delay(1000);
                    poly.StrokeWidth = 5f;
                    await Task.Delay(1000);
                    poly.StrokeWidth = 0;
                    await Task.Delay(1000);
                    poly.StrokeWidth = 2f;
                    await Task.Delay(1000);

                    poly.Color = Color.Yellow;
                    await Task.Delay(1000);

                    this._polygons.Remove(poly);
                    await Task.Delay(1000);
                    this._polygons.Add(poly);
                    await Task.Delay(1000);
                    this._polygons.Clear();
                    
                    

                    #endregion

                    #region Tiles Test

                    this.TilesUrlOptions = new TKTileUrlOptions(
                        "http://a.basemaps.cartocdn.com/dark_all/{2}/{0}/{1}.png", 256, 256, 0, 18);
                    await Task.Delay(5000);
                    this.TilesUrlOptions = null;
                    await Task.Delay(5000);
                    this.TilesUrlOptions = new TKTileUrlOptions(
                        "http://a.tile.openstreetmap.org/{2}/{0}/{1}.png", 256, 256, 0, 18);
                    #endregion

                });
            }
        }
        public Command ShowListCommand
        {
            get
            {
                return new Command(async () => 
                {
                    if(this._pins == null || !this._pins.Any())
                    {
                        Application.Current.MainPage.DisplayAlert("Nothing there!", "No pins to show!", "OK");
                        return;
                    }
                    var listPage = new PinListPage(this.Pins);
                    listPage.PinSelected += async (o, e) => 
                    {
                        this.SelectedPin = e.Pin;
                        await Application.Current.MainPage.Navigation.PopAsync();
                    };
                    await Application.Current.MainPage.Navigation.PushAsync(listPage);

                });
            }
        }
        /// <summary>
        /// Map region bound to <see cref="TKCustomMap"/>
        /// </summary>
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
        /// Pins bound to the <see cref="TKCustomMap"/>
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
        /// Routes bound to the <see cref="TKCustomMap"/>
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
        /// Circles bound to the <see cref="TKCustomMap"/>
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
        /// Lines bound to the <see cref="TKCustomMap"/>
        /// </summary>
        public ObservableCollection<TKPolyline> Lines
        {
            get { return this._lines; }
            set
            {
                if (this._lines != value)
                {
                    this._lines = value;
                    this.OnPropertyChanged("Lines");
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
        /// Map center bound to the <see cref="TKCustomMap"/>
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
        /// Selected pin bound to the <see cref="TKCustomMap"/>
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
        /// Map Long Press bound to the <see cref="TKCustomMap"/>
        /// </summary>
        public Command<Position> MapLongPressCommand
        {
            get
            {
                return new Command<Position>(async position => 
                {
                    var action = await Application.Current.MainPage.DisplayActionSheet(
                        "Long Press",
                        "Cancel",
                        null,
                        "Add Pin",
                        "Add Circle");

                    if (action == "Add Pin")
                    {
                        var pin = new TKCustomMapPin
                        {
                            Position = position,
                            Title = string.Format("Pin {0}, {1}", position.Latitude, position.Longitude),
                            ShowCallout = true,
                            IsDraggable = true
                        };
                        this._pins.Add(pin);
                    }
                    else if(action == "Add Circle")
                    {
                        var circle = new TKCircle 
                        {
                            Center = position,
                            Radius = 10000,
                            Color = Color.FromRgba(100, 0, 0, 80)
                        };
                        this._circles.Add(circle);
                    }
                    
                });
            }
        }
        /// <summary>
        /// Map Clicked bound to the <see cref="TKCustomMap"/>
        /// </summary>
        public Command<Position> MapClickedCommand
        {
            get
            {
                return new Command<Position>((positon) =>
                {
                    this.SelectedPin = null;

                    // Determine if a point was inside a circle
                    if ((from c in this._circles let distanceInMeters = c.Center.DistanceTo(positon) * 1000 where distanceInMeters <= c.Radius select c).Any())
                    {
                        Application.Current.MainPage.DisplayAlert("Circle tap", "Circle was tapped", "OK");
                    }
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
                    var gmsResult = p as GmsPlacePrediction;
                    if (gmsResult != null)
                    {
                        var details = await GmsPlace.Instance.GetDetails(gmsResult.PlaceId);
                        this.MapCenter = new Position(details.Item.Geometry.Location.Latitude, details.Item.Geometry.Location.Longitude);
                        return;
                    }
                    var osmResult = p as OsmNominatimResult;
                    if (osmResult != null)
                    {
                        this.MapCenter = new Position(osmResult.Latitude, osmResult.Longitude);
                        return;
                    }

                    if (Device.OS == TargetPlatform.Android)
                    {
                        var prediction = (TKNativeAndroidPlaceResult)p;

                        var details = await TKNativePlacesApi.Instance.GetDetails(prediction.PlaceId);

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
        /// Pin Selected bound to the <see cref="TKCustomMap"/>
        /// </summary>
        public Command PinSelectedCommand
        {
            get
            {
                return new Command<TKCustomMapPin>((TKCustomMapPin pin) =>
                {
                    // Chose one

                    // 1. First possibility
                    //this.MapCenter = this.SelectedPin.Position;
                    // 2. Possibility
                    this.MapRegion = MapSpan.FromCenterAndRadius(this.SelectedPin.Position, this.MapRegion.Radius);
                    // 3. Possibility
                    //this.MapFunctions.MoveToMapRegion(
                    //    MapSpan.FromCenterAndRadius(this.SelectedPin.Position, Distance.FromMeters(this.MapRegion.Radius.Meters)),
                    //    true);
                });
            }
        }
        /// <summary>
        /// Drag End bound to the <see cref="TKCustomMap"/>
        /// </summary>
        public Command<TKCustomMapPin> DragEndCommand
        {
            get 
            {
                return new Command<TKCustomMapPin>(pin => 
                {
                    var routePin = pin as RoutePin;

                    if (routePin != null)
                    {
                        if (routePin.IsSource)
                        {
                            routePin.Route.Source = pin.Position;
                        }
                        else
                        {
                            routePin.Route.Destination = pin.Position;
                        }
                    }
                });
            }
        }
        /// <summary>
        /// Route clicked bound to the <see cref="TKCustomMap"/>
        /// </summary>
        public Command<TKRoute> RouteClickedCommand
        {
            get
            {
                return new Command<TKRoute>(async r => 
                {
                    var action = await Application.Current.MainPage.DisplayActionSheet(
                        "Route tapped",
                        "Cancel",
                        null,
                        "Show Instructions");

                    if (action == "Show Instructions")
                    {
                        await Application.Current.MainPage.Navigation.PushAsync(new HtmlInstructionsPage(r));
                    }
                });
            }
        }
        /// <summary>
        /// Callout clicked bound to the <see cref="TKCustomMap"/>
        /// </summary>
        public Command CalloutClickedCommand
        {
            get
            {
                return new Command<TKCustomMapPin>(async (TKCustomMapPin pin) => 
                {
                    var action = await Application.Current.MainPage.DisplayActionSheet(
                        "Callout clicked",
                        "Cancel",
                        "Remove Pin");

                    if (action == "Remove Pin")
                    {
                        this._pins.Remove(pin);
                    }
                });
            }
        }
        public Command ClearMapCommand
        {
            get
            {
                return new Command(() => 
                {
                    this._pins.Clear();
                    this._circles.Clear();
                    if (this._routes != null)
                        this._routes.Clear();
                });
            }
        }
        /// <summary>
        /// Navigate to a new page to get route source/destination
        /// </summary>
        public Command AddRouteCommand
        {
            get
            {
                return new Command(() => 
                {
                    if (this.Routes == null) this.Routes = new ObservableCollection<TKRoute>();

                    var addRoutePage = new AddRoutePage(this.Routes, this.Pins, this.MapRegion);
                    Application.Current.MainPage.Navigation.PushAsync(addRoutePage);
                });
            }
        }
        /// <summary>
        /// Command when a route calculation finished
        /// </summary>
        public Command<TKRoute> RouteCalculationFinishedCommand
        {
            get
            {
                return new Command<TKRoute>(r => 
                {
                    // move to the bounds of the route
                    this.MapRegion = r.Bounds;
                });
            }
        }

        public SampleViewModel()
        {
            this._mapCenter = new Position(40.7142700, -74.0059700);

            this._pins = new ObservableCollection<TKCustomMapPin>();
            this._circles = new ObservableCollection<TKCircle>();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
