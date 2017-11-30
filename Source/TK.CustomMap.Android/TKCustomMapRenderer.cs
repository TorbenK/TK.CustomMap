using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using TK.CustomMap;
using TK.CustomMap.Api.Google;
using TK.CustomMap.Droid;
using TK.CustomMap.Interfaces;
using TK.CustomMap.Models;
using TK.CustomMap.Overlays;
using TK.CustomMap.Utilities;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Color = Xamarin.Forms.Color;
using System.Collections;
using Com.Google.Maps.Android.Clustering;
using Android.OS;

[assembly: ExportRenderer(typeof(TKCustomMap), typeof(TKCustomMapRenderer))]
namespace TK.CustomMap.Droid
{
    /// <summary>
    /// Android Renderer of <see cref="TK.CustomMap.TKCustomMap"/>
    /// </summary>
    public class TKCustomMapRenderer : ViewRenderer<TKCustomMap, MapView>, IRendererFunctions, GoogleMap.ISnapshotReadyCallback, GoogleMap.IOnCameraIdleListener, IOnMapReadyCallback
    {
        object _lockObj = new object();

        bool _isInitialized;
        bool _isLayoutPerformed;

        readonly List<TKRoute> _tempRouteList = new List<TKRoute>();

        readonly Dictionary<TKRoute, Polyline> _routes = new Dictionary<TKRoute, Polyline>();
        readonly Dictionary<TKPolyline, Polyline> _polylines = new Dictionary<TKPolyline, Polyline>();
        readonly Dictionary<TKCircle, Circle> _circles = new Dictionary<TKCircle, Circle>();
        readonly Dictionary<TKPolygon, Polygon> _polygons = new Dictionary<TKPolygon, Polygon>();
        readonly Dictionary<TKCustomMapPin, TKMarker> _markers = new Dictionary<TKCustomMapPin, TKMarker>();

        Marker _selectedMarker;
        bool _isDragging;
        bool _disposed;
        byte[] _snapShot;

        TileOverlay _tileOverlay;
        GoogleMap _googleMap;
        ClusterManager _clusterManager;

        static Bundle s_bundle;
        internal static Bundle Bundle { set { s_bundle = value; } }

        GoogleMap Map => _googleMap;
        internal TKCustomMap FormsMap
        {
            get { return Element as TKCustomMap; }
        }
        IMapFunctions MapFunctions
        {
            get { return Element as IMapFunctions; }
        }


        /// <inheritdoc />
        protected override void OnElementChanged(ElementChangedEventArgs<TKCustomMap> e)
        {
            if (!TKGoogleMaps.IsInitialized) throw new Exception("Call TKGoogleMaps.Init first");

            var oldMapView = Control;
            var mapView = new MapView(Context);
            mapView.OnCreate(s_bundle);
            mapView.OnResume();
            SetNativeControl(mapView);

            lock (_lockObj)
            {
                base.OnElementChanged(e);

                if (mapView == null) return;

                if (e.OldElement != null)
                {
                    e.OldElement.PropertyChanged -= FormsMapPropertyChanged;
                    UnregisterCollections((TKCustomMap)e.OldElement);

                    if (_googleMap != null)
                    {
                        _googleMap.MarkerClick -= OnMarkerClick;
                        _googleMap.MapClick -= OnMapClick;
                        _googleMap.MapLongClick -= OnMapLongClick;
                        _googleMap.MarkerDragEnd -= OnMarkerDragEnd;
                        _googleMap.MarkerDrag -= OnMarkerDrag;
                        _googleMap.MarkerDragStart -= OnMarkerDragStart;
                        _googleMap.InfoWindowClick -= OnInfoWindowClick;
                        _googleMap.MyLocationChange -= OnUserLocationChange;
                        _googleMap.SetOnCameraIdleListener(null);

                        _googleMap = null;
                    }

                }


                if (e.NewElement != null)
                {
                    MapFunctions.SetRenderer(this);
                    mapView.GetMapAsync(this);

                    FormsMap.PropertyChanged += FormsMapPropertyChanged;

                }
            }

        }
        ///<inheritdoc/>
        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            if (!_isLayoutPerformed)
            {
                _isLayoutPerformed = true;
                UpdateMapRegion();
                _isInitialized = true;
            }
        }
        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;

            _disposed = true;

            if (disposing)
            {
                if (FormsMap != null)
                {
                    FormsMap.PropertyChanged -= FormsMapPropertyChanged;
                    UnregisterCollections(FormsMap);
                }
                if (_googleMap != null)
                {
                    _googleMap.MarkerClick -= OnMarkerClick;
                    _googleMap.MapClick -= OnMapClick;
                    _googleMap.MapLongClick -= OnMapLongClick;
                    _googleMap.MarkerDragEnd -= OnMarkerDragEnd;
                    _googleMap.MarkerDrag -= OnMarkerDrag;
                    _googleMap.MarkerDragStart -= OnMarkerDragStart;
                    _googleMap.InfoWindowClick -= OnInfoWindowClick;
                    _googleMap.MyLocationChange -= OnUserLocationChange;
                    _googleMap.SetOnCameraIdleListener(null);

                    _clusterManager?.Dispose();
                    _clusterManager = null;
                    _googleMap.Dispose();
                    _googleMap = null;
                }
            }

            base.Dispose(disposing);
        }
        /// <summary>
        /// When a property of the Forms map changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void FormsMapPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_googleMap == null) return;

            switch(e.PropertyName)
            {
                case nameof(TKCustomMap.Pins):
                    UpdatePins();
                    break;
                case nameof(TKCustomMap.SelectedPin):
                    SetSelectedItem();
                    break;
                case nameof(TKCustomMap.Polylines):
                    UpdateLines();
                    break;
                case nameof(TKCustomMap.Circles):
                    UpdateCircles();
                    break;
                case nameof(TKCustomMap.Polygons):
                    UpdatePolygons();
                    break;
                case nameof(TKCustomMap.RoutesProperty):
                    UpdateRoutes();
                    break;
                case nameof(TKCustomMap.TilesUrlOptions):
                    UpdateTileOptions();
                    break;
                case nameof(TKCustomMap.ShowTraffic):
                    UpdateShowTraffic();
                    break;
                case nameof(TKCustomMap.MapRegion):
                    UpdateMapRegion();
                    break;
                case nameof(TKCustomMap.IsClusteringEnabled):
                    UpdateIsClusteringEnabled();
                    break;
                case nameof(TKCustomMap.MapType):
                    UpdateMapType();
                    break;
                case nameof(TKCustomMap.IsShowingUser):
                    UpdateIsShowingUser();
                    break;
                case nameof(TKCustomMap.HasScrollEnabled):
                    UpdateHasScrollEnabled();
                    break;
                case nameof(TKCustomMap.HasZoomEnabled):
                    UpdateHasZoomEnabled();
                    break;
            }
        }
        /// <summary>
        /// When the map is ready to use
        /// </summary>
        /// <param name="googleMap">The map instance</param>
        public virtual void OnMapReady(GoogleMap googleMap)
        {
            lock (_lockObj)
            {
                _googleMap = googleMap;

                if (FormsMap.IsClusteringEnabled)
                {
                    _clusterManager = new ClusterManager(Context, _googleMap);
                    _clusterManager.Renderer = new TKMarkerRenderer(Context, _googleMap, _clusterManager, this);
                }

                _googleMap.MarkerClick += OnMarkerClick;
                _googleMap.MapClick += OnMapClick;
                _googleMap.MapLongClick += OnMapLongClick;
                _googleMap.MarkerDragEnd += OnMarkerDragEnd;
                _googleMap.MarkerDrag += OnMarkerDrag;
                _googleMap.MarkerDragStart += OnMarkerDragStart;
                _googleMap.InfoWindowClick += OnInfoWindowClick;
                _googleMap.MyLocationChange += OnUserLocationChange;

                _googleMap.SetOnCameraIdleListener(this);

                UpdateTileOptions();
                UpdateMapRegion();
                UpdatePins();
                UpdateRoutes();
                UpdateLines();
                UpdateCircles();
                UpdatePolygons();
                UpdateShowTraffic();
                UpdateMapType();
                UpdateIsShowingUser();
                UpdateHasZoomEnabled();
                UpdateHasScrollEnabled();

                MapFunctions.RaiseMapReady();
            }
        }
        /// <summary>
        /// When the location of the user changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnUserLocationChange(object sender, GoogleMap.MyLocationChangeEventArgs e)
        {
            if (e.Location == null || FormsMap == null) return;

            var newPosition = new Position(e.Location.Latitude, e.Location.Longitude);
            MapFunctions.RaiseUserLocationChanged(newPosition);
        }
        /// <summary>
        /// When the info window gets clicked
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnInfoWindowClick(object sender, GoogleMap.InfoWindowClickEventArgs e)
        {
            var pin = GetPinByMarker(e.Marker);

            if (pin == null) return;

            if (pin.IsCalloutClickable)
                MapFunctions.RaiseCalloutClicked(pin);
        }
        /// <summary>
        /// Dragging process
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnMarkerDrag(object sender, GoogleMap.MarkerDragEventArgs e)
        {
            var item = _markers.SingleOrDefault(i => i.Value.Marker.Id.Equals(e.Marker.Id));
            if (item.Key == null) return;

            item.Key.Position = e.Marker.Position.ToPosition();
        }
        /// <summary>
        /// When a dragging starts
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnMarkerDragStart(object sender, GoogleMap.MarkerDragStartEventArgs e)
        {
            _isDragging = true;
        }
        /// <summary>
        /// When the camera position changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnCameraChange(object sender, GoogleMap.CameraChangeEventArgs e)
        {
            if (FormsMap == null) return;

            FormsMap.MapRegion = GetCurrentMapRegion(e.Position.Target);

            if (FormsMap.IsClusteringEnabled)
            {
                _clusterManager.OnCameraIdle();
            }
        }
        /// <summary>
        /// When a pin gets clicked
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnMarkerClick(object sender, GoogleMap.MarkerClickEventArgs e)
        {
            if (FormsMap == null) return;
            var item = _markers.SingleOrDefault(i => i.Value.Marker.Id.Equals(e.Marker.Id));
            if (item.Key == null) return;

            _selectedMarker = e.Marker;
            FormsMap.SelectedPin = item.Key;
            if (item.Key.ShowCallout)
            {
                item.Value.Marker.ShowInfoWindow();
            }
        }
        /// <summary>
        /// When a drag of a marker ends
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnMarkerDragEnd(object sender, GoogleMap.MarkerDragEndEventArgs e)
        {
            _isDragging = false;

            if (FormsMap == null) return;

            var pin = _markers.SingleOrDefault(i => i.Value.Marker.Id.Equals(e.Marker.Id));
            if (pin.Key == null) return;

            if (FormsMap.IsClusteringEnabled)
            {
                _clusterManager.RemoveItem(pin.Value);
                _clusterManager.AddItem(pin.Value);
                _clusterManager.Cluster();
            }

            MapFunctions.RaisePinDragEnd(pin.Key);
        }
        /// <summary>
        /// When a long click was performed on the map
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnMapLongClick(object sender, GoogleMap.MapLongClickEventArgs e)
        {
            if (FormsMap == null) return;

            var position = e.Point.ToPosition();
            MapFunctions.RaiseMapLongPress(position);
        }
        /// <summary>
        /// When the map got tapped
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnMapClick(object sender, GoogleMap.MapClickEventArgs e)
        {
            if (FormsMap == null) return;

            var position = e.Point.ToPosition();

            if (FormsMap.Routes != null)
            {
                foreach (var route in FormsMap.Routes.Where(i => i.Selectable))
                {
                    var internalRoute = _routes[route];

                    if (GmsPolyUtil.IsLocationOnPath(
                        position,
                        internalRoute.Points.Select(i => i.ToPosition()),
                        true,
                        (int)_googleMap.CameraPosition.Zoom,
                        FormsMap.MapCenter.Latitude))
                    {
                        MapFunctions.RaiseRouteClicked(route);
                        return;
                    }
                }
            }
            MapFunctions.RaiseMapClicked(position);
        }
        /// <summary>
        /// Updates the markers when a pin gets added or removed in the collection
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnCustomPinsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
                    if (!FormsMap.Pins.Contains(pin))
                    {
                        RemovePin(pin);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                UpdatePins(false);
            }
        }
        /// <summary>
        /// When a property of a pin changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        async void OnPinPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var pin = sender as TKCustomMapPin;
            if (pin == null) return;


            TKMarker marker = null;
            if (!_markers.ContainsKey(pin) || (marker = _markers[pin]) == null) return;
            await marker.HandlePropertyChangedAsync(e, _isDragging);

            if (FormsMap.IsClusteringEnabled && e.PropertyName == nameof(TKCustomMapPin.Position) && !_isDragging)
            {
                _clusterManager.RemoveItem(marker);
                _clusterManager.AddItem(marker);
                _clusterManager.Cluster();
            }
        }
        /// <summary>
        /// Collection of routes changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnLineCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
                        _polylines[line].Remove();
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
        void OnLinePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var line = (TKPolyline)sender;

            if (e.PropertyName == nameof(TKPolyline.LineCoordinates))
            {
                if (line.LineCoordinates != null && line.LineCoordinates.Count > 1)
                {
                    _polylines[line].Points = new List<LatLng>(line.LineCoordinates.Select(i => i.ToLatLng()));
                }
                else
                {
                    _polylines[line].Points = null;
                }
            }
            else if (e.PropertyName == nameof(TKPolyline.Color))
            {
                _polylines[line].Color = line.Color.ToAndroid().ToArgb();
            }
            else if (e.PropertyName == nameof(TKPolyline.LineWidth))
            {
                _polylines[line].Width = line.LineWidth;
            }
        }
        /// <summary>
        /// Creates all Markers on the map
        /// </summary>
        void UpdatePins(bool firstUpdate = true)
        {
            if (_googleMap == null) return;

            foreach (var i in _markers)
            {
                RemovePin(i.Key, false);
            }
            _markers.Clear();
            if (FormsMap.Pins != null)
            {
                foreach (var pin in FormsMap.Pins)
                {
                    AddPin(pin);
                }
                if (firstUpdate)
                {
                    var observAble = FormsMap.Pins as INotifyCollectionChanged;
                    if (observAble != null)
                    {
                        observAble.CollectionChanged += OnCustomPinsCollectionChanged;
                    }
                }
                MapFunctions.RaisePinsReady();
            }
        }
        /// <summary>
        /// Adds a marker to the map
        /// </summary>
        /// <param name="pin">The Forms Pin</param>
        async void AddPin(TKCustomMapPin pin)
        {
            if (_markers.Keys.Contains(pin)) return;

            pin.PropertyChanged += OnPinPropertyChanged;

            var tkMarker = new TKMarker(pin, Context);
            var markerWithIcon = new MarkerOptions();
            await tkMarker.InitializeMarkerOptionsAsync(markerWithIcon);

            _markers.Add(pin, tkMarker);

            if (FormsMap.IsClusteringEnabled)
            {
                _clusterManager.AddItem(tkMarker);
                _clusterManager.Cluster();
            }
            else
            {
                tkMarker.Marker = _googleMap.AddMarker(markerWithIcon);
            }

        }
        /// <summary>
        /// Remove a pin from the map and the internal dictionary
        /// </summary>
        /// <param name="pin">The pin to remove</param>
        /// <param name="removeMarker">true to remove the marker from the map</param>
        void RemovePin(TKCustomMapPin pin, bool removeMarker = true)
        {
            if (!_markers.TryGetValue(pin, out var item)) return;

            if (_selectedMarker != null)
            {
                if (item.Marker.Id.Equals(_selectedMarker.Id))
                {
                    FormsMap.SelectedPin = null;
                }
            }

            _clusterManager?.RemoveItem(item);
            item.Marker?.Remove();
            pin.PropertyChanged -= OnPinPropertyChanged;

            if (removeMarker)
            {
                _markers.Remove(pin);
            }
        }
        /// <summary>
        /// Set the selected item on the map
        /// </summary>
        void SetSelectedItem()
        {
            if (_selectedMarker != null)
            {
                _selectedMarker.HideInfoWindow();
                _selectedMarker = null;
            }
            if (FormsMap.SelectedPin != null)
            {
                if (!_markers.ContainsKey(FormsMap.SelectedPin)) return;

                var selectedPin = _markers[FormsMap.SelectedPin];
                _selectedMarker = selectedPin.Marker;
                if (FormsMap.SelectedPin.ShowCallout)
                {
                    selectedPin.Marker.ShowInfoWindow();
                }
                MapFunctions.RaisePinSelected(FormsMap.SelectedPin);
            }
        }

        /// <summary>
        /// Creates the routes on the map
        /// </summary>
        void UpdateLines(bool firstUpdate = true)
        {
            if (_googleMap == null) return;

            foreach (var i in _polylines)
            {
                i.Key.PropertyChanged -= OnLinePropertyChanged;
                i.Value.Remove();
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
        /// Updates all circles
        /// </summary>
        void UpdateCircles(bool firstUpdate = true)
        {
            if (_googleMap == null) return;

            foreach (var i in _circles)
            {
                i.Key.PropertyChanged -= CirclePropertyChanged;
                i.Value.Remove();
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
        /// Creates the polygones on the map
        /// </summary>
        /// <param name="firstUpdate">If the collection updates the first time</param>
        void UpdatePolygons(bool firstUpdate = true)
        {
            if (_googleMap == null) return;

            foreach (var i in _polygons)
            {
                i.Key.PropertyChanged -= OnPolygonPropertyChanged;
                i.Value.Remove();
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
        /// Create all routes
        /// </summary>
        /// <param name="firstUpdate">If first update of collection or not</param>
        void UpdateRoutes(bool firstUpdate = true)
        {
            _tempRouteList.Clear();

            if (_googleMap == null) return;

            foreach (var i in _routes)
            {
                if (i.Key != null)
                    i.Key.PropertyChanged -= OnRoutePropertyChanged;
                i.Value.Remove();
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
        void OnRouteCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
                        _routes[route].Remove();
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
        void OnRoutePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var route = (TKRoute)sender;

            if (e.PropertyName == nameof(TKRoute.Source) ||
                e.PropertyName == nameof(TKRoute.Destination) ||
                e.PropertyName == nameof(TKRoute.TravelMode))
            {
                route.PropertyChanged -= OnRoutePropertyChanged;
                _routes[route].Remove();
                _routes.Remove(route);

                AddRoute(route);
            }
            else if (e.PropertyName == nameof(TKPolyline.Color))
            {
                _routes[route].Color = route.Color.ToAndroid().ToArgb();
            }
            else if (e.PropertyName == nameof(TKPolyline.LineWidth))
            {
                _routes[route].Width = route.LineWidth;
            }
        }
        /// <summary>
        /// When the polygon collection changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnPolygonsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
                        _polygons[poly].Remove();
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
        void AddPolygon(TKPolygon polygon)
        {
            polygon.PropertyChanged += OnPolygonPropertyChanged;

            var polygonOptions = new PolygonOptions();

            if (polygon.Coordinates != null && polygon.Coordinates.Any())
            {
                polygonOptions.Add(polygon.Coordinates.Select(i => i.ToLatLng()).ToArray());
            }
            if (polygon.Color != Color.Default)
            {
                polygonOptions.InvokeFillColor(polygon.Color.ToAndroid().ToArgb());
            }
            if (polygon.StrokeColor != Color.Default)
            {
                polygonOptions.InvokeStrokeColor(polygon.StrokeColor.ToAndroid().ToArgb());
            }
            polygonOptions.InvokeStrokeWidth(polygon.StrokeWidth);

            _polygons.Add(polygon, _googleMap.AddPolygon(polygonOptions));
        }
        /// <summary>
        /// When a property of a <see cref="TKPolygon"/> changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnPolygonPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var tkPolygon = (TKPolygon)sender;

            switch (e.PropertyName)
            {
                case nameof(TKPolygon.Coordinates):
                    _polygons[tkPolygon].Points = tkPolygon.Coordinates.Select(i => i.ToLatLng()).ToList();
                    break;
                case nameof(TKPolygon.Color):
                    _polygons[tkPolygon].FillColor = tkPolygon.Color.ToAndroid().ToArgb();
                    break;
                case nameof(TKPolygon.StrokeColor):
                    _polygons[tkPolygon].StrokeColor = tkPolygon.StrokeColor.ToAndroid().ToArgb();
                    break;
                case nameof(TKPolygon.StrokeWidth):
                    _polygons[tkPolygon].StrokeWidth = tkPolygon.StrokeWidth;
                    break;
            }
        }
        /// <summary>
        /// When the circle collection changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void CirclesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
                        _circles[circle].Remove();
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
        void AddCircle(TKCircle circle)
        {
            circle.PropertyChanged += CirclePropertyChanged;

            var circleOptions = new CircleOptions();

            circleOptions.InvokeRadius(circle.Radius);
            circleOptions.InvokeCenter(circle.Center.ToLatLng());

            if (circle.Color != Color.Default)
            {
                circleOptions.InvokeFillColor(circle.Color.ToAndroid().ToArgb());
            }
            if (circle.StrokeColor != Color.Default)
            {
                circleOptions.InvokeStrokeColor(circle.StrokeColor.ToAndroid().ToArgb());
            }
            circleOptions.InvokeStrokeWidth(circle.StrokeWidth);
            _circles.Add(circle, _googleMap.AddCircle(circleOptions));
        }
        /// <summary>
        /// When a property of a <see cref="TKCircle"/> changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void CirclePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var tkCircle = (TKCircle)sender;
            var circle = _circles[tkCircle];

            switch (e.PropertyName)
            {
                case nameof(TKCircle.Radius):
                    circle.Radius = tkCircle.Radius;
                    break;
                case nameof(TKCircle.Center):
                    circle.Center = tkCircle.Center.ToLatLng();
                    break;
                case nameof(TKCircle.Color):
                    circle.FillColor = tkCircle.Color.ToAndroid().ToArgb();
                    break;
                case nameof(TKCircle.StrokeColor):
                    circle.StrokeColor = tkCircle.StrokeColor.ToAndroid().ToArgb();
                    break;
            }
        }
        /// <summary>
        /// Adds a route to the map
        /// </summary>
        /// <param name="line">The route to add</param>
        void AddLine(TKPolyline line)
        {
            line.PropertyChanged += OnLinePropertyChanged;

            var polylineOptions = new PolylineOptions();
            if (line.Color != Color.Default)
            {
                polylineOptions.InvokeColor(line.Color.ToAndroid().ToArgb());
            }
            if (line.LineWidth > 0)
            {
                polylineOptions.InvokeWidth(line.LineWidth);
            }

            if (line.LineCoordinates != null)
            {
                polylineOptions.Add(line.LineCoordinates.Select(i => i.ToLatLng()).ToArray());
            }

            _polylines.Add(line, _googleMap.AddPolyline(polylineOptions));
        }
        /// <summary>
        /// Calculates and adds the route to the map
        /// </summary>
        /// <param name="route">The route to add</param>
        async void AddRoute(TKRoute route)
        {
            if (route == null) return;

            _tempRouteList.Add(route);

            route.PropertyChanged += OnRoutePropertyChanged;

            GmsDirectionResult routeData = null;
            string errorMessage = null;

            routeData = await GmsDirection.Instance.CalculateRoute(route.Source, route.Destination, route.TravelMode.ToGmsTravelMode());

            if (FormsMap == null || Map == null || !_tempRouteList.Contains(route)) return;

            if (routeData != null && routeData.Routes != null)
            {
                if (routeData.Status == GmsDirectionResultStatus.Ok)
                {
                    var r = routeData.Routes.FirstOrDefault();
                    if (r != null && r.Polyline.Positions != null && r.Polyline.Positions.Any())
                    {
                        SetRouteData(route, r);

                        var routeOptions = new PolylineOptions();

                        if (route.Color != Color.Default)
                        {
                            routeOptions.InvokeColor(route.Color.ToAndroid().ToArgb());
                        }
                        if (route.LineWidth > 0)
                        {
                            routeOptions.InvokeWidth(route.LineWidth);
                        }
                        routeOptions.Add(r.Polyline.Positions.Select(i => i.ToLatLng()).ToArray());

                        _routes.Add(route, _googleMap.AddPolyline(routeOptions));

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
        /// <param name="routeResult">The rourte api result</param>
        void SetRouteData(TKRoute route, GmsRouteResult routeResult)
        {
            var latLngBounds = new LatLngBounds(
                    new LatLng(routeResult.Bounds.SouthWest.Latitude, routeResult.Bounds.SouthWest.Longitude),
                    new LatLng(routeResult.Bounds.NorthEast.Latitude, routeResult.Bounds.NorthEast.Longitude));

            var apiSteps = routeResult.Legs.First().Steps;
            var steps = new TKRouteStep[apiSteps.Count()];
            var routeFunctions = (IRouteFunctions)route;


            for (int i = 0; i < steps.Length; i++)
            {
                steps[i] = new TKRouteStep();
                var stepFunctions = (IRouteStepFunctions)steps[i];
                var apiStep = apiSteps.ElementAt(i);

                stepFunctions.SetDistance(apiStep.Distance.Value);
                stepFunctions.SetInstructions(apiStep.HtmlInstructions);
            }
            routeFunctions.SetSteps(steps);
            routeFunctions.SetDistance(routeResult.Legs.First().Distance.Value);
            routeFunctions.SetTravelTime(routeResult.Legs.First().Duration.Value);

            routeFunctions.SetBounds(
                MapSpan.FromCenterAndRadius(
                    latLngBounds.Center.ToPosition(),
                    Distance.FromKilometers(
                        new Position(latLngBounds.Southwest.Latitude, latLngBounds.Southwest.Longitude)
                        .DistanceTo(
                            new Position(latLngBounds.Northeast.Latitude, latLngBounds.Northeast.Longitude)) / 2)));
            routeFunctions.SetIsCalculated(true);
        }
        /// <summary>
        /// Updates the image of a pin
        /// </summary>
        /// <param name="pin">The forms pin</param>
        /// <param name="markerOptions">The native marker options</param>
        async Task UpdateImage(TKCustomMapPin pin, MarkerOptions markerOptions)
        {
            BitmapDescriptor bitmap;
            try
            {
                if (pin.Image != null)
                {
                    bitmap = BitmapDescriptorFactory.FromBitmap(await pin.Image.ToBitmap(Context));
                }
                else
                {
                    if (pin.DefaultPinColor != Color.Default)
                    {
                        var hue = pin.DefaultPinColor.ToAndroid().GetHue();
                        bitmap = BitmapDescriptorFactory.DefaultMarker(Math.Min(hue, 359.99f));
                    }
                    else
                    {
                        bitmap = BitmapDescriptorFactory.DefaultMarker();
                    }
                }
            }
            catch (Exception)
            {
                bitmap = BitmapDescriptorFactory.DefaultMarker();
            }
            markerOptions.SetIcon(bitmap);
        }
        /// <summary>
        /// Updates the image on a marker
        /// </summary>
        /// <param name="pin">The forms pin</param>
        /// <param name="marker">The native marker</param>
        async Task UpdateImage(TKCustomMapPin pin, Marker marker)
        {
            BitmapDescriptor bitmap;
            try
            {
                if (pin.Image != null)
                {
                    bitmap = BitmapDescriptorFactory.FromBitmap(await pin.Image.ToBitmap(Context));
                }
                else
                {
                    if (pin.DefaultPinColor != Color.Default)
                    {
                        var hue = pin.DefaultPinColor.ToAndroid().GetHue();
                        bitmap = BitmapDescriptorFactory.DefaultMarker(hue);
                    }
                    else
                    {
                        bitmap = BitmapDescriptorFactory.DefaultMarker();
                    }
                }
            }
            catch (Exception)
            {
                bitmap = BitmapDescriptorFactory.DefaultMarker();
            }
            marker.SetIcon(bitmap);
        }
        /// <summary>
        /// Updates the custom tile provider 
        /// </summary>
        void UpdateTileOptions()
        {
            if (_tileOverlay != null)
            {
                _tileOverlay.Remove();
                _googleMap.MapType = GoogleMap.MapTypeNormal;
            }

            if (FormsMap == null || _googleMap == null) return;

            if (FormsMap.TilesUrlOptions != null)
            {
                _googleMap.MapType = GoogleMap.MapTypeNone;

                _tileOverlay = _googleMap.AddTileOverlay(
                    new TileOverlayOptions()
                        .InvokeTileProvider(
                            new TKCustomTileProvider(FormsMap.TilesUrlOptions))
                        .InvokeZIndex(-1));
            }
        }
        /// <summary>
        /// Updates the visible map region
        /// </summary>
        void UpdateMapRegion()
        {
            if (FormsMap == null || _googleMap == null || !_isLayoutPerformed) return;

            if (!FormsMap.MapRegion.Equals(GetCurrentMapRegion(_googleMap.CameraPosition.Target)))
            {
                MoveToMapRegion(FormsMap.MapRegion, FormsMap.IsRegionChangeAnimated);
            }
        }
        /// <summary>
        /// Sets traffic enabled on the google map
        /// </summary>
        void UpdateShowTraffic()
        {
            if (FormsMap == null || _googleMap == null) return;

            _googleMap.TrafficEnabled = FormsMap.ShowTraffic;
        }
        /// <summary>
        /// Updates clustering
        /// </summary>
        void UpdateIsClusteringEnabled()
        {
            if (FormsMap == null || _googleMap == null) return;

            if (FormsMap.IsClusteringEnabled)
            {
                if (_clusterManager == null)
                {
                    _clusterManager = new ClusterManager(Context, _googleMap);
                    _clusterManager.Renderer = new TKMarkerRenderer(Context, _googleMap, _clusterManager, this);
                }
                foreach (var marker in _markers.ToList())
                {
                    RemovePin(marker.Key);
                    AddPin(marker.Key);
                }
                _clusterManager.Cluster();
            }
            else
            {
                foreach (var marker in _markers.ToList())
                {
                    RemovePin(marker.Key);
                    AddPin(marker.Key);
                }
                _clusterManager.Cluster();

                _clusterManager.Dispose();
                _clusterManager = null;
            }
        }
        /// <summary>
        /// Updates the map type
        /// </summary>
        void UpdateMapType()
        {
            if (FormsMap == null || _googleMap == null) return;

            switch(FormsMap.MapType)
            {
                case MapType.Hybrid:
                    Map.MapType = GoogleMap.MapTypeHybrid;
                    break;
                case MapType.Satellite:
                    Map.MapType = GoogleMap.MapTypeSatellite;
                    break;
                case MapType.Street:
                    Map.MapType = GoogleMap.MapTypeNormal;
                    break;
            }
        }
        /// <summary>
        /// Updates if the user location and user location button are displayed
        /// </summary>
        void UpdateIsShowingUser()
        {
            if (FormsMap == null || _googleMap == null) return;

            Map.MyLocationEnabled = Map.UiSettings.MyLocationButtonEnabled = FormsMap.IsShowingUser;   
        }
        /// <summary>
        /// Updates scroll gesture
        /// </summary>
        void UpdateHasScrollEnabled()
        {
            if (FormsMap == null || _googleMap == null) return;

            Map.UiSettings.ScrollGesturesEnabled = FormsMap.HasScrollEnabled;
        }
        /// <summary>
        /// Updates zoom gesture/control
        /// </summary>
        void UpdateHasZoomEnabled()
        {
            if (FormsMap == null || _googleMap == null) return;

            Map.UiSettings.ZoomGesturesEnabled = Map.UiSettings.ZoomControlsEnabled = FormsMap.HasZoomEnabled;
        }
        /// <summary>
        /// Creates a <see cref="LatLngBounds"/> from a collection of <see cref="MapSpan"/>
        /// </summary>
        /// <param name="spans">The spans to get calculate the bounds from</param>
        /// <returns>The bounds</returns>
        LatLngBounds BoundsFromMapSpans(params MapSpan[] spans)
        {
            LatLngBounds.Builder builder = new LatLngBounds.Builder();

            foreach (var region in spans)
            {
                builder
                    .Include(GmsSphericalUtil.ComputeOffset(region.Center, region.Radius.Meters, 0).ToLatLng())
                    .Include(GmsSphericalUtil.ComputeOffset(region.Center, region.Radius.Meters, 90).ToLatLng())
                    .Include(GmsSphericalUtil.ComputeOffset(region.Center, region.Radius.Meters, 180).ToLatLng())
                    .Include(GmsSphericalUtil.ComputeOffset(region.Center, region.Radius.Meters, 270).ToLatLng());
            }
            return builder.Build();
        }
        /// <summary>
        /// Unregisters all collections
        /// </summary>
        void UnregisterCollections(TKCustomMap map)
        {
            UnregisterCollection(map.Pins, OnCustomPinsCollectionChanged, OnPinPropertyChanged);
            UnregisterCollection(map.Routes, OnRouteCollectionChanged, OnRoutePropertyChanged);
            UnregisterCollection(map.Polylines, OnLineCollectionChanged, OnLinePropertyChanged);
            UnregisterCollection(map.Circles, CirclesCollectionChanged, CirclePropertyChanged);
            UnregisterCollection(map.Polygons, OnPolygonsCollectionChanged, OnPolygonPropertyChanged);
        }
        /// <summary>
        /// Unregisters one collection and all of its items
        /// </summary>
        /// <param name="collection">The collection to unregister</param>
        /// <param name="observableHandler">The <see cref="NotifyCollectionChangedEventHandler"/> of the collection</param>
        /// <param name="propertyHandler">The <see cref="PropertyChangedEventHandler"/> of the collection items</param>
        void UnregisterCollection(
           IEnumerable collection,
           NotifyCollectionChangedEventHandler observableHandler,
           PropertyChangedEventHandler propertyHandler)
        {
            if (collection == null) return;

            var observable = collection as INotifyCollectionChanged;
            if (observable != null)
            {
                observable.CollectionChanged -= observableHandler;
            }
            foreach (INotifyPropertyChanged obj in collection)
            {
                obj.PropertyChanged -= propertyHandler;
            }
        }
        /// <summary>
        /// Gets the current mapregion
        /// </summary>
        /// <param name="center">Center point</param>
        /// <returns>The map region</returns>
        MapSpan GetCurrentMapRegion(LatLng center)
        {
            var map = _googleMap;
            if (map == null)
                return null;

            var projection = map.Projection;
            var width = Control.Width;
            var height = Control.Height;
            var ul = projection.FromScreenLocation(new global::Android.Graphics.Point(0, 0));
            var ur = projection.FromScreenLocation(new global::Android.Graphics.Point(width, 0));
            var ll = projection.FromScreenLocation(new global::Android.Graphics.Point(0, height));
            var lr = projection.FromScreenLocation(new global::Android.Graphics.Point(width, height));
            var dlat = Math.Max(Math.Abs(ul.Latitude - lr.Latitude), Math.Abs(ur.Latitude - ll.Latitude));
            var dlong = Math.Max(Math.Abs(ul.Longitude - lr.Longitude), Math.Abs(ur.Longitude - ll.Longitude));

            return new MapSpan(new Position(center.Latitude, center.Longitude), dlat, dlong);
        }
        /// <inheritdoc/>
        public async Task<byte[]> GetSnapshot()
        {
            if (_googleMap == null) return null;

            _snapShot = null;
            _googleMap.Snapshot(this);

            while (_snapShot == null) await Task.Delay(10);

            return _snapShot;
        }
        ///<inheritdoc/>
        public void OnSnapshotReady(Bitmap snapshot)
        {
            using (var strm = new MemoryStream())
            {
                snapshot.Compress(Bitmap.CompressFormat.Png, 100, strm);
                _snapShot = strm.ToArray();
            }
        }
        ///<inheritdoc/>
        public void FitMapRegionToPositions(IEnumerable<Position> positions, bool animate = false, int padding = 0)
        {
            if (_googleMap == null) throw new InvalidOperationException("Map not ready");
            if (positions == null) throw new InvalidOperationException("positions can't be null");

            LatLngBounds.Builder builder = new LatLngBounds.Builder();

            positions.ToList().ForEach(i => builder.Include(i.ToLatLng()));

            if (animate)
                _googleMap.AnimateCamera(CameraUpdateFactory.NewLatLngBounds(builder.Build(), padding));
            else
                _googleMap.MoveCamera(CameraUpdateFactory.NewLatLngBounds(builder.Build(), padding));
        }
        ///<inheritdoc/>
        public void MoveToMapRegion(MapSpan region, bool animate)
        {
            if (_googleMap == null) return;

            if (region == null) return;

            var bounds = BoundsFromMapSpans(region);
            if (bounds == null) return;
            var cam = CameraUpdateFactory.NewLatLngBounds(bounds, 0);

            if (animate && _isInitialized)
                _googleMap.AnimateCamera(cam);
            else
                _googleMap.MoveCamera(cam);
        }
        ///<inheritdoc/>
        public void FitToMapRegions(IEnumerable<MapSpan> regions, bool animate = false, int padding = 0)
        {
            if (_googleMap == null || regions == null || !regions.Any()) return;

            var bounds = BoundsFromMapSpans(regions.ToArray());
            if (bounds == null) return;
            var cam = CameraUpdateFactory.NewLatLngBounds(bounds, padding);

            if (animate)
                _googleMap.AnimateCamera(cam);
            else
                _googleMap.MoveCamera(cam);
        }
        ///<inheritdoc/>
        public IEnumerable<Position> ScreenLocationsToGeocoordinates(params Xamarin.Forms.Point[] screenLocations)
        {
            if (_googleMap == null)
                throw new InvalidOperationException("Map not initialized");

            return screenLocations.Select(i => _googleMap.Projection.FromScreenLocation(i.ToAndroidPoint()).ToPosition());
        }
        /// <summary>
        /// Gets the <see cref="TKCustomMapPin"/> by the native <see cref="Marker"/>
        /// </summary>
        /// <param name="marker">The marker to search the pin for</param>
        /// <returns>The forms pin</returns>
        protected TKCustomMapPin GetPinByMarker(Marker marker)
        {
            return _markers.SingleOrDefault(i => i.Value.Marker.Id == marker.Id).Key;
        }

        public void OnCameraIdle()
        {
            if (FormsMap == null) return;

            FormsMap.MapRegion = GetCurrentMapRegion(Map.CameraPosition.Target);

            if (FormsMap.IsClusteringEnabled)
            {
                _clusterManager.OnCameraIdle();
            }
        }
    }
}
