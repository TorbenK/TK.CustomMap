using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TK.CustomMap;
using TK.CustomMap.Interfaces;
using TK.CustomMap.Models;
using TK.CustomMap.Overlays;
using TK.CustomMap.UWP;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media.Imaging;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.UWP;
using Xamarin.Forms.Platform.UWP;
using Point = Xamarin.Forms.Point;

[assembly: ExportRenderer(typeof(TKCustomMap), typeof(TKCustomMapRenderer))]

namespace TK.CustomMap.UWP
{
    /// <summary>
    /// UWP Renderer of <see cref="TK.CustomMap.TKCustomMap"/>
    /// </summary>
    [Preserve(AllMembers = true)]
    public class TKCustomMapRenderer : MapRenderer, IRendererFunctions
    {
        private const int EarthRadiusInMeteres = 6371000;

        private readonly List<TKRoute> _tempRouteList = new List<TKRoute>();

        private readonly Dictionary<TKRoute, MapPolyline> _routes = new Dictionary<TKRoute, MapPolyline>();
        private readonly Dictionary<TKPolyline, MapPolyline> _polylines = new Dictionary<TKPolyline, MapPolyline>();
        private readonly Dictionary<TKCustomMapPin, TKCustomBingMapsPin> _pins = new Dictionary<TKCustomMapPin, TKCustomBingMapsPin>();
        private readonly Dictionary<TKCircle, MapPolygon> _circles = new Dictionary<TKCircle, MapPolygon>();
        private readonly Dictionary<TKPolygon, MapPolygon> _polygons = new Dictionary<TKPolygon, MapPolygon>();


        private MapControl Map
        {
            get { return Control as MapControl; }
        }

        private TKCustomMap FormsMap
        {
            get { return Element as TKCustomMap; }
        }

        private IMapFunctions MapFunctions
        {
            get { return Element as IMapFunctions; }
        }

        /// <summary>
        /// Gets/Sets if the default pin drop animation is enabled
        /// </summary>
        public static bool AnimateOnPinDrop { get; set; } = true;

        /// <summary>
        /// Dummy function to avoid linker.
        /// </summary>
        [Preserve]
        public static void InitMapRenderer()
        {
            var temp = DateTime.Now;
        }

        /// <inheritdoc/>
        protected override void OnElementChanged(ElementChangedEventArgs<Map> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null && Map != null)
            {
                e.OldElement.PropertyChanged -= OnMapPropertyChanged;

                Map.MapHolding -= Map_MapHolding;
                Map.MapTapped -= Map_MapTapped;
            }

            if (e.NewElement != null)
            {
                MapFunctions.SetRenderer(this);

                Map.MapHolding += Map_MapHolding;
                Map.MapTapped += Map_MapTapped;

                //this.UpdateTileOptions();
                SetMapCenter();
                UpdatePins();
                UpdateRoutes();
                UpdateLines();
                UpdateCircles();
                UpdatePolygons();
                UpdateShowTraffic();
                FormsMap.PropertyChanged += OnMapPropertyChanged;
            }
        }


        private void Map_MapTapped(MapControl sender, MapInputEventArgs args)
        {
            MapFunctions.RaiseMapClicked(args.Location.ToPosition());
        }

        private void Map_MapHolding(MapControl sender, MapInputEventArgs args)
        {
            MapFunctions.RaiseMapLongPress(args.Location.ToPosition());
        }

        public async void FitMapRegionToPositions(IEnumerable<Position> positions, bool animate = false)
        {
            if (Map == null) throw new InvalidOperationException("Map not ready");
            if (positions == null) throw new InvalidOperationException("Positions can't be null");

            var bounds = GeoboundingBox.TryCompute(positions.Select(i => new BasicGeoposition
            {
                Latitude = i.Latitude,
                Longitude = i.Longitude
            }));

            if (bounds == null) return;

            await Map.TrySetViewBoundsAsync(bounds, null, animate ? MapAnimationKind.Bow : MapAnimationKind.None);
        }

        public void FitToMapRegions(IEnumerable<MapSpan> regions, bool animate)
        {
            throw new NotImplementedException();
            //if (Map == null) return;

            //Rect rect;

            //foreach (var region in regions)
            //{
            //    rect = Rect.Union(
            //        rect,
            //        this.RegionToRect(
            //            MKCoordinateRegion.FromDistance(
            //                region.Center.ToLocationCoordinate(),
            //                region.Radius.Meters * 2,
            //                region.Radius.Meters * 2)));
            //}
            //Map.SetVisibleMapRect(rect, new UIEdgeInsets(15, 15, 15, 15), animate);
        }

        public async Task<byte[]> GetSnapshot()
        {
            if (Map == null) return null;

            var rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(Map);
            var pixelBuffer = await rtb.GetPixelsAsync();

            return pixelBuffer.ToArray();
        }

        public async void MoveToMapRegion(MapSpan region, bool animate)
        {
            if (Map == null) return;

            var animation = animate ? MapAnimationKind.Bow : MapAnimationKind.None;

            await Map.TrySetViewAsync(region.ToLocationCoordinate(), Map.ZoomLevel, Map.Heading, Map.Pitch, animation);
        }

        public IEnumerable<Position> ScreenLocationsToGeocoordinates(params Point[] screenLocations)
        {
            if (Map == null)
                throw new InvalidOperationException("Map not initialized");

            var resultPositions = new List<Position>();

            foreach (var l in screenLocations)
            {
                Geopoint tempPosition;

                Map.GetLocationFromOffset(l.ToUWPPoint(), out tempPosition);

                if (tempPosition != null)
                    resultPositions.Add(tempPosition.ToPosition());
            }

            return resultPositions;
        }
        
        /// <summary>
        /// When a property of the forms map changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnMapPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TKCustomMap.CustomPinsProperty.PropertyName)
            {
                UpdatePins();
            }
            else if (e.PropertyName == TKCustomMap.SelectedPinProperty.PropertyName)
            {
                //this.SetSelectedPin();
            }
            else if (e.PropertyName == TKCustomMap.MapCenterProperty.PropertyName)
            {
                SetMapCenter();
            }
            else if (e.PropertyName == TKCustomMap.PolylinesProperty.PropertyName)
            {
                UpdateLines();
            }
            else if (e.PropertyName == TKCustomMap.CirclesProperty.PropertyName)
            {
                UpdateCircles();
            }
            else if (e.PropertyName == TKCustomMap.CalloutClickedCommandProperty.PropertyName)
            {
                UpdatePins(false);
            }
            else if (e.PropertyName == TKCustomMap.PolygonsProperty.PropertyName)
            {
                UpdatePolygons();
            }
            else if (e.PropertyName == TKCustomMap.RoutesProperty.PropertyName)
            {
                UpdateRoutes();
            }
            else if (e.PropertyName == TKCustomMap.TilesUrlOptionsProperty.PropertyName)
            {
                //this.UpdateTileOptions();
            }
            else if (e.PropertyName == TKCustomMap.ShowTrafficProperty.PropertyName)
            {
                UpdateShowTraffic();
            }
            else if (e.PropertyName == TKCustomMap.MapRegionProperty.PropertyName)
            {
                UpdateMapRegion();
            }
        }

        /// <summary>
        /// Creates the annotations
        /// </summary>
        private void UpdatePins(bool firstUpdate = true)
        {
            var pins = Map.Children.OfType<TKCustomBingMapsPin>().ToList();

            foreach (var pin in pins) Map.Children.Remove(pin);

            _pins.Clear();
            if (FormsMap.CustomPins == null) return;

            foreach (var i in FormsMap.CustomPins)
            {
                AddPin(i);
            }

            if (firstUpdate)
            {
                var observAble = FormsMap.CustomPins as INotifyCollectionChanged;
                if (observAble != null)
                {
                    observAble.CollectionChanged += OnCollectionChanged;
                }
            }

            MapFunctions.RaisePinsReady();
        }

        /// <summary>
        /// When the collection of pins changed
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (TKCustomMapPin pin in e.NewItems)
                {
                    AddPin(pin);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (TKCustomMapPin pin in e.OldItems)
                {
                    if (!FormsMap.CustomPins.Contains(pin))
                    {
                        if (FormsMap.SelectedPin != null && FormsMap.SelectedPin.Equals(pin))
                        {
                            FormsMap.SelectedPin = null;
                        }

                        var pinControl = _pins[pin];

                        if (pinControl != null)
                        {
                            pinControl.Observe(false);
                            Map.Children.Remove(pinControl);
                            _pins.Remove(pin);
                        }
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var pin in Map.Children.OfType<TKCustomBingMapsPin>())
                {
                    pin.Observe(false);
                }

                UpdatePins(false);
            }
        }
        
     

        /// <summary>
        /// Adds a pin
        /// </summary>
        /// <param name="pin">The pin to add</param>
        private void AddPin(TKCustomMapPin pin)
        {
            var pinControl = new TKCustomBingMapsPin(pin, Map);
            Map.Children.Add(pinControl);
            _pins.Add(pin, pinControl);

            pinControl.Observe(true);
        }

        /// <summary>
        /// Sets the center of the map
        /// </summary>
        private void SetMapCenter()
        {
            if (FormsMap == null || Map == null) return;

            if (!FormsMap.MapCenter.Equals(Map.Center.ToPosition()))
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    var animation = FormsMap.IsRegionChangeAnimated ? MapAnimationKind.Bow : MapAnimationKind.None;

                    await Map.TrySetViewAsync(FormsMap.MapCenter.ToLocationCoordinate(), Map.ZoomLevel, Map.Heading, Map.Pitch, animation);
                    Map.Center = FormsMap.MapCenter.ToLocationCoordinate();
                });
            }
        }

        /// <summary>
        /// Updates the map region when changed
        /// </summary>
        private void UpdateMapRegion()
        {
            if (FormsMap == null || FormsMap.MapRegion == null) return;

            if (FormsMap.MapRegion != FormsMap.VisibleRegion)
            {
                MoveToMapRegion(FormsMap.MapRegion, FormsMap.IsRegionChangeAnimated);
            }
        }

        /// <summary>
        /// Sets traffic enabled on the map
        /// </summary>
        private void UpdateShowTraffic()
        {
            if (FormsMap == null || Map == null) return;

            Map.TrafficFlowVisible = FormsMap.ShowTraffic;
        }

        /// <summary>
        /// Creates the polygones on the map
        /// </summary>
        /// <param name="firstUpdate">If the collection updates the first time</param>
        private void UpdatePolygons(bool firstUpdate = true)
        {
            // TODO on different
            if (Map == null) return;

            foreach (var i in _polygons)
            {
                i.Key.PropertyChanged -= OnPolygonPropertyChanged;
                Map.MapElements.Remove(i.Value);
            }

            _polygons.Clear();

            if (FormsMap.Polygons != null)
            {
                foreach (var i in FormsMap.Polygons)
                {
                    AddPolygon(i);
                }
                if (firstUpdate)
                {
                    var observAble = FormsMap.Polygons as INotifyCollectionChanged;
                    if (observAble != null)
                    {
                        observAble.CollectionChanged += OnPolygonsCollectionChanged;
                    }
                }
            }
        }

        /// <summary>
        /// When the polygon collection changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnPolygonsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (TKPolygon poly in e.NewItems)
                {
                    AddPolygon(poly);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (TKPolygon poly in e.OldItems)
                {
                    if (!FormsMap.Polygons.Contains(poly))
                    {
                        Map.MapElements.Remove(_polygons[poly]);
                        poly.PropertyChanged -= OnPolygonPropertyChanged;
                        _polygons.Remove(poly);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                UpdatePolygons(false);
            }
        }

        /// <summary>
        /// Adds a polygon to the map
        /// </summary>
        /// <param name="polygon">The polygon to add</param>
        private void AddPolygon(TKPolygon polygon)
        {
            polygon.PropertyChanged += OnPolygonPropertyChanged;

            var mapPolygon = new MapPolygon();

            if (polygon.Coordinates != null && polygon.Coordinates.Any())
            {
                mapPolygon.Path = new Geopath(polygon.Coordinates.Select(i => new BasicGeoposition
                {
                    Latitude = i.Latitude,
                    Longitude = i.Longitude
                }));
            }

            if (polygon.Color != Color.Default)
            {
                mapPolygon.FillColor = polygon.Color.ToUWPColor();
            }

            if (polygon.StrokeColor != Color.Default)
            {
                mapPolygon.StrokeColor = polygon.StrokeColor.ToUWPColor();
            }

            mapPolygon.StrokeThickness = polygon.StrokeWidth;

            Map.MapElements.Add(mapPolygon);
            _polygons.Add(polygon, mapPolygon);
        }

        /// <summary>
        /// When a property of a <see cref="TKPolygon"/> changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnPolygonPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var tkPolygon = (TKPolygon)sender;

            switch (e.PropertyName)
            {
                case TKPolygon.CoordinatesPropertyName:
                    _polygons[tkPolygon].Path = new Geopath(tkPolygon.Coordinates.Select(i => new BasicGeoposition
                    {
                        Latitude = i.Latitude,
                        Longitude = i.Longitude
                    }));
                    break;

                case TKPolygon.ColorPropertyName:
                    _polygons[tkPolygon].FillColor = tkPolygon.Color.ToUWPColor();
                    break;

                case TKPolygon.StrokeColorPropertyName:
                    _polygons[tkPolygon].StrokeColor = tkPolygon.StrokeColor.ToUWPColor();
                    break;

                case TKPolygon.StrokeWidthPropertyName:
                    _polygons[tkPolygon].StrokeThickness = tkPolygon.StrokeWidth;
                    break;
            }
        }

        /// <summary>
        /// Collection of routes changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnLineCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (TKPolyline line in e.NewItems)
                {
                    AddLine(line);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (TKPolyline line in e.OldItems)
                {
                    if (!FormsMap.Polylines.Contains(line))
                    {
                        Map.MapElements.Remove(_polylines[line]);
                        line.PropertyChanged -= OnLinePropertyChanged;
                        _polylines.Remove(line);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                UpdateLines(false);
            }
        }

        /// <summary>
        /// A property of a route changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnLinePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var line = (TKPolyline)sender;

            if (e.PropertyName == TKPolyline.LineCoordinatesPropertyName)
            {
                if (line.LineCoordinates != null && line.LineCoordinates.Count > 1)
                {
                    _polylines[line].Path = new Geopath(line.LineCoordinates.Select(i => new BasicGeoposition
                    {
                        Latitude = i.Latitude,
                        Longitude = i.Longitude
                    }));
                }
                else
                {
                    _polylines[line].Path = null;
                }
            }
            else if (e.PropertyName == TKPolyline.ColorPropertyName)
            {
                _polylines[line].StrokeColor = line.Color.ToUWPColor();
            }
            else if (e.PropertyName == TKPolyline.LineWidthProperty)
            {
                _polylines[line].StrokeThickness = line.LineWidth;
            }
        }

        /// <summary>
        /// Creates the routes on the map
        /// </summary>
        private void UpdateLines(bool firstUpdate = true)
        {
            if (Map == null) return;

            foreach (var i in _polylines)
            {
                i.Key.PropertyChanged -= OnLinePropertyChanged;
                Map.MapElements.Remove(i.Value);
            }

            _polylines.Clear();

            if (FormsMap.Polylines != null)
            {
                foreach (var line in FormsMap.Polylines)
                {
                    AddLine(line);
                }

                if (firstUpdate)
                {
                    var observAble = FormsMap.Polylines as INotifyCollectionChanged;
                    if (observAble != null)
                    {
                        observAble.CollectionChanged += OnLineCollectionChanged;
                    }
                }
            }
        }

        /// <summary>
        /// Adds a line to the map
        /// </summary>
        /// <param name="line">The line to add</param>
        private void AddLine(TKPolyline line)
        {
            line.PropertyChanged += OnLinePropertyChanged;

            var polyline = new MapPolyline();

            if (line.Color != Color.Default)
            {
                polyline.StrokeColor = line.Color.ToUWPColor();
            }
            if (line.LineWidth > 0)
            {
                polyline.StrokeThickness = line.LineWidth;
            }

            if (line.LineCoordinates != null)
            {
                polyline.Path = new Geopath(line.LineCoordinates.Select(i => new BasicGeoposition
                {
                    Latitude = i.Latitude,
                    Longitude = i.Longitude
                }));
            }

            Map.MapElements.Add(polyline);
            _polylines.Add(line, polyline);
        }

        /// <summary>
        /// Updates all circles
        /// </summary>
        private void UpdateCircles(bool firstUpdate = true)
        {
            if (Map == null) return;

            foreach (var i in _circles)
            {
                i.Key.PropertyChanged -= CirclePropertyChanged;
                Map.MapElements.Remove(i.Value);
            }

            _circles.Clear();

            if (FormsMap.Circles != null)
            {
                foreach (var circle in FormsMap.Circles)
                {
                    AddCircle(circle);
                }
                if (firstUpdate)
                {
                    var observAble = FormsMap.Circles as INotifyCollectionChanged;
                    if (observAble != null)
                    {
                        observAble.CollectionChanged += CirclesCollectionChanged;
                    }
                }
            }
        }

        /// <summary>
        /// When the circle collection changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void CirclesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (TKCircle circle in e.NewItems)
                {
                    AddCircle(circle);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (TKCircle circle in e.OldItems)
                {
                    if (!FormsMap.Circles.Contains(circle))
                    {
                        circle.PropertyChanged -= CirclePropertyChanged;
                        Map.MapElements.Remove(_circles[circle]);
                        _circles.Remove(circle);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                UpdateCircles(false);
            }
        }

        /// <summary>
        /// Adds a circle to the map
        /// </summary>
        /// <param name="circle">The circle to add</param>
        private void AddCircle(TKCircle circle)
        {
            circle.PropertyChanged += CirclePropertyChanged;

            var circleCoordinates = GenerateCircleCoordinates(circle.Center, circle.Radius);

            var polyCircle = new MapPolygon
            {
                Path = new Geopath(circleCoordinates.Select(i => new BasicGeoposition
                {
                    Latitude = i.Latitude,
                    Longitude = i.Longitude
                }))
            };

            if (circle.Color != Color.Default)
            {
                polyCircle.FillColor = circle.Color.ToUWPColor();
            }

            if (circle.StrokeColor != Color.Default)
            {
                polyCircle.StrokeColor = circle.StrokeColor.ToUWPColor();
            }

            polyCircle.StrokeThickness = circle.StrokeWidth;

            Map.MapElements.Add(polyCircle);
            _circles.Add(circle, polyCircle);
        }

        /// <summary>
        /// When a property of a <see cref="TKCircle"/> changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void CirclePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var tkCircle = (TKCircle)sender;
            var circle = _circles[tkCircle];

            switch (e.PropertyName)
            {
                case TKCircle.CenterPropertyName:
                case TKCircle.RadiusPropertyName:
                    var circleCoordinates = GenerateCircleCoordinates(tkCircle.Center, tkCircle.Radius);

                    circle.Path = new Geopath(circleCoordinates.Select(i => new BasicGeoposition
                    {
                        Latitude = i.Latitude,
                        Longitude = i.Longitude
                    }));
                    break;

                case TKCircle.ColorPropertyName:
                    circle.FillColor = tkCircle.Color.ToUWPColor();
                    break;

                case TKCircle.StrokeColorPropertyName:
                    circle.StrokeColor = tkCircle.StrokeColor.ToUWPColor();
                    break;
            }
        }

        private List<Position> GenerateCircleCoordinates(Position position, double radius)
        {
            var latitude = position.Latitude.ToRadians();
            var longitude = position.Longitude.ToRadians();
            var distance = radius / EarthRadiusInMeteres;
            var positions = new List<Position>();

            for (var angle = 0; angle <= 360; angle++)
            {
                var angleInRadians = ((double)angle).ToRadians();
                var latitudeInRadians = Math.Asin(Math.Sin(latitude) * Math.Cos(distance) + Math.Cos(latitude) * Math.Sin(distance) * Math.Cos(angleInRadians));
                var longitudeInRadians = longitude + Math.Atan2(Math.Sin(angleInRadians) * Math.Sin(distance) * Math.Cos(latitude), Math.Cos(distance) - Math.Sin(latitude) * Math.Sin(latitudeInRadians));

                var pos = new Position(latitudeInRadians.ToDegrees(), longitudeInRadians.ToDegrees());
                positions.Add(pos);
            }

            return positions;
        }

        /// <summary>
        /// Create all routes
        /// </summary>
        /// <param name="firstUpdate">If first update of collection or not</param>
        private void UpdateRoutes(bool firstUpdate = true)
        {
            _tempRouteList.Clear();

            if (Map == null) return;

            foreach (var i in _routes)
            {
                if (i.Key != null)
                    i.Key.PropertyChanged -= OnRoutePropertyChanged;

                Map.MapElements.Remove(i.Value);
            }

            _routes.Clear();

            if (FormsMap == null || FormsMap.Routes == null) return;

            foreach (var i in FormsMap.Routes)
            {
                AddRoute(i);
            }

            if (firstUpdate)
            {
                var observAble = FormsMap.Routes as INotifyCollectionChanged;
                if (observAble != null)
                {
                    observAble.CollectionChanged += OnRouteCollectionChanged;
                }
            }
        }

        /// <summary>
        /// When the collection of routes changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnRouteCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (TKRoute route in e.NewItems)
                {
                    AddRoute(route);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (TKRoute route in e.OldItems)
                {
                    if (!FormsMap.Routes.Contains(route))
                    {
                        Map.MapElements.Remove(_routes[route]);
                        route.PropertyChanged -= OnRoutePropertyChanged;
                        _routes.Remove(route);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                UpdateRoutes(false);
            }
        }

        /// <summary>
        /// When a property of a route changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnRoutePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var route = (TKRoute)sender;

            if (e.PropertyName == TKRoute.SourceProperty ||
                e.PropertyName == TKRoute.DestinationProperty ||
                e.PropertyName == TKRoute.TravelModelProperty)
            {
                route.PropertyChanged -= OnRoutePropertyChanged;
                Map.MapElements.Remove(_routes[route]);
                _routes.Remove(route);

                AddRoute(route);
            }
            else if (e.PropertyName == TKPolyline.ColorPropertyName)
            {
                _routes[route].StrokeColor = route.Color.ToUWPColor();
            }
            else if (e.PropertyName == TKPolyline.LineWidthProperty)
            {
                _routes[route].StrokeThickness = route.LineWidth;
            }
        }

        /// <summary>
        /// Calculates and adds the route to the map
        /// </summary>
        /// <param name="route">The route to add</param>
        private async void AddRoute(TKRoute route)
        {
            if (route == null) return;

            _tempRouteList.Add(route);

            route.PropertyChanged += OnRoutePropertyChanged;

            MapRouteFinderResult routeData;
            string errorMessage = null;

            switch (route.TravelMode)
            {
                case TKRouteTravelMode.Walking:
                    routeData = await MapRouteFinder.GetWalkingRouteAsync(
                route.Source.ToLocationCoordinate(), route.Destination.ToLocationCoordinate());
                    break;

                default:
                    routeData = await MapRouteFinder.GetDrivingRouteAsync(
                route.Source.ToLocationCoordinate(), route.Destination.ToLocationCoordinate(),
                MapRouteOptimization.Time, MapRouteRestrictions.None);
                    break;
            }

            if (FormsMap == null || Map == null || !_tempRouteList.Contains(route)) return;

            if (routeData != null && routeData.Route != null)
            {
                if (routeData.Status == MapRouteFinderStatus.Success)
                {
                    var r = routeData.Route;
                    if (r != null && r.Path.Positions != null && r.Path.Positions.Any())
                    {
                        SetRouteData(route, r);

                        var polyline = new MapPolyline();

                        if (route.Color != Color.Default)
                        {
                            polyline.StrokeColor = route.Color.ToUWPColor();
                        }
                        if (route.LineWidth > 0)
                        {
                            polyline.StrokeThickness = route.LineWidth;
                        }

                        if (r.Path != null)
                        {
                            polyline.Path = new Geopath(r.Path.Positions.Select(i => new BasicGeoposition
                            {
                                Latitude = i.Latitude,
                                Longitude = i.Longitude
                            }));
                        }

                        Map.MapElements.Add(polyline);
                        _routes.Add(route, polyline);

                        MapFunctions.RaiseRouteCalculationFinished(route);
                    }
                    else
                    {
                        errorMessage = "Unexpected result";
                    }
                }
                else
                {
                    errorMessage = routeData.Status.ToString();
                }
            }
            else
            {
                errorMessage = "Could not connect to api";
            }
            if (!string.IsNullOrEmpty(errorMessage))
            {
                var routeCalculationError = new TKRouteCalculationError(route, errorMessage);

                MapFunctions.RaiseRouteCalculationFailed(routeCalculationError);
            }
        }

        /// <summary>
        /// Sets the route calculation data
        /// </summary>
        /// <param name="route">The PCL route</param>
        /// <param name="routeResult">The route api result</param>
        private void SetRouteData(TKRoute route, MapRoute routeResult)
        {
            var latLngBounds = routeResult.BoundingBox;

            var apiSteps = routeResult.Legs.First().Maneuvers;
            var steps = new TKRouteStep[apiSteps.Count];
            var routeFunctions = (IRouteFunctions)route;

            for (int i = 0; i < steps.Length; i++)
            {
                steps[i] = new TKRouteStep();
                var stepFunctions = (IRouteStepFunctions)steps[i];
                var apiStep = apiSteps.ElementAt(i);

                stepFunctions.SetDistance(apiStep.LengthInMeters);
                stepFunctions.SetInstructions(apiStep.InstructionText);
            }
            routeFunctions.SetSteps(steps);
            routeFunctions.SetDistance(routeResult.Legs.First().LengthInMeters);
            routeFunctions.SetTravelTime(routeResult.Legs.First().EstimatedDuration.TotalMinutes);

            routeFunctions.SetBounds(
                MapSpan.FromCenterAndRadius(
                    latLngBounds.Center.ToPosition(),
                    Distance.FromKilometers(
                        new Position(latLngBounds.SoutheastCorner.Latitude, latLngBounds.SoutheastCorner.Longitude)
                        .DistanceTo(
                            new Position(latLngBounds.NorthwestCorner.Latitude, latLngBounds.NorthwestCorner.Longitude)) / 2)));

            routeFunctions.SetIsCalculated(true);
        }
        protected virtual TKCustomBingMapsPin GetPinControlByPin(TKCustomMapPin pin)
        {
            return _pins[pin];
        }

        public void FitMapRegionToPositions(IEnumerable<Position> positions, bool animate = false, int padding = 0)
        {
            throw new NotImplementedException();
        }

        public void FitToMapRegions(IEnumerable<MapSpan> regions, bool animate = false, int padding = 0)
        {
            throw new NotImplementedException();
        }
    }
}