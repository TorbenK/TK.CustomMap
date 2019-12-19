using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using CoreLocation;
using Foundation;
using MapKit;
using ObjCRuntime;
using TK.CustomMap;
using TK.CustomMap.iOSUnified;
using TK.CustomMap.Interfaces;
using TK.CustomMap.Models;
using TK.CustomMap.Overlays;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System.Collections;
using Xamarin.iOS.ClusterKit;
// ReSharper disable InconsistentNaming
// ReSharper disable SuspiciousTypeConversion.Global

[assembly: ExportRenderer(typeof(TKCustomMap), typeof(TKCustomMapRenderer))]

namespace TK.CustomMap.iOSUnified
{
    /// <summary>
    /// iOS Renderer of <see cref="TK.CustomMap.TKCustomMap"/>
    /// </summary>
    [Preserve(AllMembers = true)]
    public class TKCustomMapRenderer : ViewRenderer<TKCustomMap, MKMapView>, IRendererFunctions
    {
        const double MercatorRadius = 85445659.44705395;
        const string AnnotationIdentifier = "TKCustomAnnotation";
        const string AnnotationIdentifierDefaultPin = "TKCustomAnnotationDefaultPin";
        const string AnnotationIdentifierDefaultClusterPin = "TKDefaultClusterPin";

        readonly List<TKRoute> _tempRouteList = new List<TKRoute>();

        readonly Dictionary<MKPolyline, TKOverlayItem<TKRoute, MKPolylineRenderer>> _routes = new Dictionary<MKPolyline, TKOverlayItem<TKRoute, MKPolylineRenderer>>();
        readonly Dictionary<MKPolyline, TKOverlayItem<TKPolyline, MKPolylineRenderer>> _lines = new Dictionary<MKPolyline, TKOverlayItem<TKPolyline, MKPolylineRenderer>>();
        readonly Dictionary<MKCircle, TKOverlayItem<TKCircle, MKCircleRenderer>> _circles = new Dictionary<MKCircle, TKOverlayItem<TKCircle, MKCircleRenderer>>();
        readonly Dictionary<MKPolygon, TKOverlayItem<TKPolygon, MKPolygonRenderer>> _polygons = new Dictionary<MKPolygon, TKOverlayItem<TKPolygon, MKPolygonRenderer>>();

        bool _isDragging;
        bool _disposed;
        IMKAnnotation _selectedAnnotation;
        MKTileOverlay _tileOverlay;
        MKTileOverlayRenderer _tileOverlayRenderer;
        UIGestureRecognizer _longPressGestureRecognizer;
        UIGestureRecognizer _tapGestureRecognizer;
        UIGestureRecognizer _doubleTapGestureRecognizer;
        CLLocationManager _locationManager;
        TKClusterMap _clusterMap;
        private TKOverlayItem<TKPolygon, MKPolygonRenderer> _polygonRenderer;
        private TKOverlayItem<TKRoute, MKPolylineRenderer> _routeRenderer;
        private TKOverlayItem<TKPolyline, MKPolylineRenderer> _lineRenderer;
        private TKOverlayItem<TKCircle, MKCircleRenderer> _circleRenderer;

        MKMapView Map => Control as MKMapView;

        TKCustomMap FormsMap => Element as TKCustomMap;

        IMapFunctions MapFunctions => Element as IMapFunctions;

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
            // ReSharper disable once UnusedVariable
            var temp = DateTime.Now;
        }
        /// <inheritdoc/>
        protected override void OnElementChanged(ElementChangedEventArgs<TKCustomMap> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null && Map != null)
            {
                e.OldElement.PropertyChanged -= OnMapPropertyChanged;

                Map.GetViewForAnnotation = null;
                Map.OverlayRenderer = null;
                Map.DidSelectAnnotationView -= OnDidSelectAnnotationView;
                Map.RegionChanged -= OnMapRegionChanged;
                Map.DidUpdateUserLocation -= OnDidUpdateUserLocation;
                Map.ChangedDragState -= OnChangedDragState;
                Map.CalloutAccessoryControlTapped -= OnMapCalloutAccessoryControlTapped;
                UnregisterCollections(e.OldElement);

                Map.RemoveGestureRecognizer(_longPressGestureRecognizer);
                Map.RemoveGestureRecognizer(_tapGestureRecognizer);
                Map.RemoveGestureRecognizer(_doubleTapGestureRecognizer);
                _longPressGestureRecognizer.Dispose();
                _tapGestureRecognizer.Dispose();
                _doubleTapGestureRecognizer.Dispose();
            }

            if (e.NewElement == null) return;
            
            if (Control == null)
            {
                var mapView = new MKMapView();
                SetNativeControl(mapView);
            }

            MapFunctions.SetRenderer(this);

            if (FormsMap.IsClusteringEnabled)
            {
                _clusterMap = new TKClusterMap(Map);
            }

            if (Map == null) return;
            
            Map.GetViewForAnnotation = GetViewForAnnotation;
            Map.OverlayRenderer = GetOverlayRenderer;
            Map.DidSelectAnnotationView += OnDidSelectAnnotationView;
            Map.RegionChanged += OnMapRegionChanged;
            Map.DidUpdateUserLocation += OnDidUpdateUserLocation;
            Map.ChangedDragState += OnChangedDragState;
            Map.CalloutAccessoryControlTapped += OnMapCalloutAccessoryControlTapped;

            Map.AddGestureRecognizer((_longPressGestureRecognizer = new UILongPressGestureRecognizer(OnMapLongPress)));

            _doubleTapGestureRecognizer = new UITapGestureRecognizer() { NumberOfTapsRequired = 2 };

            _tapGestureRecognizer = new UITapGestureRecognizer(OnMapClicked);
            _tapGestureRecognizer.RequireGestureRecognizerToFail(_doubleTapGestureRecognizer);
            _tapGestureRecognizer.ShouldReceiveTouch = (recognizer, touch) => !(touch.View is MKAnnotationView);

            Map.AddGestureRecognizer(_tapGestureRecognizer);
            Map.AddGestureRecognizer(_doubleTapGestureRecognizer);

            UpdateTileOptions();
            UpdatePins();
            UpdateRoutes();
            UpdateLines();
            UpdateCircles();
            UpdatePolygons();
            UpdateShowTraffic();
            UpdateMapRegion();
            UpdateMapType();
            UpdateIsShowingUser();
            UpdateHasScrollEnabled();
            UpdateHasZoomEnabled();
            FormsMap.PropertyChanged += OnMapPropertyChanged;

            MapFunctions.RaiseMapReady();
        }

        /// <summary>
        /// Get the overlay renderer
        /// </summary>
        /// <param name="mapView">The <see cref="MKMapView"/></param>
        /// <param name="overlay">The overlay to render</param>
        /// <returns>The overlay renderer</returns>
        MKOverlayRenderer GetOverlayRenderer(MKMapView mapView, IMKOverlay overlay)
        {
            switch (overlay)
            {
                case MKPolyline polyline when _routes.TryGetValue(polyline, out var route):
                {
                    _routeRenderer = route;
                    if (_routeRenderer.Renderer == null)
                    {
                        _routeRenderer.Renderer = new MKPolylineRenderer(polyline);
                    }

                    _routeRenderer.Renderer.FillColor = route.Overlay.Color.ToUIColor();
                    _routeRenderer.Renderer.LineWidth = route.Overlay.LineWidth;
                    _routeRenderer.Renderer.StrokeColor = route.Overlay.Color.ToUIColor();
                    return _routeRenderer.Renderer;
                }
                case MKPolyline polyline when _lines.TryGetValue(polyline, out var line):
                {
                    _lineRenderer = line;
                    if (_lineRenderer.Renderer == null)
                    {
                        _lineRenderer.Renderer = new MKPolylineRenderer(polyline);
                    }

                    _lineRenderer.Renderer.FillColor = _lineRenderer.Overlay.Color.ToUIColor();
                    _lineRenderer.Renderer.LineWidth = _lineRenderer.Overlay.LineWidth;
                    _lineRenderer.Renderer.StrokeColor = _lineRenderer.Overlay.Color.ToUIColor();

                    // return renderer for the line
                    return _lineRenderer.Renderer;
                }
                case MKCircle mkCircle:
                {
                    _circleRenderer = _circles[mkCircle];

                    if (_circleRenderer.Renderer == null)
                    {
                        _circleRenderer.Renderer = new MKCircleRenderer(mkCircle);
                    }
                    _circleRenderer.Renderer.FillColor = _circleRenderer.Overlay.Color.ToUIColor();
                    _circleRenderer.Renderer.StrokeColor = _circleRenderer.Overlay.StrokeColor.ToUIColor();
                    _circleRenderer.Renderer.LineWidth = _circleRenderer.Overlay.StrokeWidth;
                    return _circleRenderer.Renderer;
                }
                case MKPolygon mkPolygon:
                {
                    _polygonRenderer = _polygons[mkPolygon];

                    if (_polygonRenderer.Renderer == null)
                    {
                        _polygonRenderer.Renderer = new MKPolygonRenderer(mkPolygon);
                    }

                    _polygonRenderer.Renderer.FillColor = _polygonRenderer.Overlay.Color.ToUIColor();
                    _polygonRenderer.Renderer.StrokeColor = _polygonRenderer.Overlay.StrokeColor.ToUIColor();
                    _polygonRenderer.Renderer.LineWidth = _polygonRenderer.Overlay.StrokeWidth;
                    return _polygonRenderer.Renderer;
                }
                case MKTileOverlay _:
                    _tileOverlayRenderer?.Dispose();

                    return (_tileOverlayRenderer = new MKTileOverlayRenderer(_tileOverlay));
                default:
                    return null;
            }
        }
        /// <summary>
        /// When the user location changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnDidUpdateUserLocation(object sender, MKUserLocationEventArgs e)
        {
            if (e.UserLocation == null || FormsMap == null || FormsMap.UserLocationChangedCommand == null) return;

            var newPosition = e.UserLocation.Location.Coordinate.ToPosition();
            MapFunctions.RaiseUserLocationChanged(newPosition);
        }
        /// <summary>
        /// When a property of the forms map changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnMapPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(TKCustomMap.Pins):
                    UpdatePins();
                    break;
                case nameof(TKCustomMap.SelectedPin):
                    SetSelectedPin();
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
                case nameof(TKCustomMap.Routes):
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
        /// When the collection of pins changed
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
                List<TKCustomMapAnnotation> annotationsToRemove = new List<TKCustomMapAnnotation>();

                foreach (TKCustomMapPin pin in e.OldItems)
                {
                    if (!FormsMap.Pins.Contains(pin))
                    {
                        if (FormsMap.SelectedPin != null && FormsMap.SelectedPin.Equals(pin))
                        {
                            FormsMap.SelectedPin = null;
                        }

                        var annotation = GetCustomAnnotation(pin);

                        if (annotation != null)
                        {
                            annotation.CustomPin.PropertyChanged -= OnPinPropertyChanged;
                            annotationsToRemove.Add(annotation);
                        }
                    }
                }

                if (FormsMap.IsClusteringEnabled)
                {
                    _clusterMap.ClusterManager.RemoveAnnotations(annotationsToRemove.Cast<MKAnnotation>().ToArray());
                }
                else
                {
                    Map.RemoveAnnotations(annotationsToRemove.Cast<IMKAnnotation>().ToArray());
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                IEnumerable<IMKAnnotation> annotations = FormsMap.IsClusteringEnabled ? _clusterMap.ClusterManager.Annotations : Map.Annotations;

                foreach (var annotation in annotations.OfType<TKCustomMapAnnotation>())
                {
                    annotation.CustomPin.PropertyChanged -= OnPinPropertyChanged;
                }
                UpdatePins(false);
            }
        }
        /// <summary>
        /// When the accessory control of a callout gets tapped
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnMapCalloutAccessoryControlTapped(object sender, MKMapViewAccessoryTappedEventArgs e)
        {
            MapFunctions.RaiseCalloutClicked(GetPinByAnnotation(e.View.Annotation));
        }
        /// <summary>
        /// When the drag state changed
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event Arguments</param>
        void OnChangedDragState(object sender, MKMapViewDragStateEventArgs e)
        {
            var annotation = GetCustomAnnotation(e.AnnotationView);
            if (annotation == null) return;

            if (e.NewState == MKAnnotationViewDragState.Starting)
            {
                _isDragging = true;
            }
            else if (e.NewState == MKAnnotationViewDragState.Dragging)
            {
                annotation.CustomPin.Position = e.AnnotationView.Annotation.Coordinate.ToPosition();
            }
            else if (e.NewState == MKAnnotationViewDragState.Ending || e.NewState == MKAnnotationViewDragState.Canceling)
            {

                if (FormsMap.IsClusteringEnabled)
                {
                    var ckCluster = Runtime.GetNSObject(e.AnnotationView.Annotation.Handle) as CKCluster;
                    if (!(ckCluster is null))
                    {
                        annotation.SetCoordinateInternal(ckCluster.Coordinate, true);
                    }
                }

                if (!(e.AnnotationView is MKPinAnnotationView))
                    e.AnnotationView.DragState = MKAnnotationViewDragState.None;
                _isDragging = false;
                MapFunctions.RaisePinDragEnd(annotation.CustomPin);
            }
        }
        /// <summary>
        /// When the camera region changed
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event Arguments</param>
        void OnMapRegionChanged(object sender, MKMapViewChangeEventArgs e)
        {
            FormsMap.MapRegion = Map.GetCurrentMapRegion();

            if (FormsMap.IsClusteringEnabled)
            {
                _clusterMap.ClusterManager.UpdateClustersIfNeeded();
            }
        }
        /// <summary>
        /// When an annotation view got selected
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        public virtual void OnDidSelectAnnotationView(object sender, MKAnnotationViewEventArgs e)
        {
            var pin = GetCustomAnnotation(e.View);

            if (pin == null) return;

            _selectedAnnotation = e.View.Annotation;
            FormsMap.SelectedPin = pin.CustomPin;

            MapFunctions.RaisePinSelected(pin.CustomPin);
        }
        /// <summary>
        /// When a tap was perfomed on the map
        /// </summary>
        /// <param name="recognizer">The gesture recognizer</param>
        void OnMapClicked(UITapGestureRecognizer recognizer)
        {
            if (recognizer.State != UIGestureRecognizerState.Ended) return;

            var pixelLocation = recognizer.LocationInView(Map);
            var coordinate = Map.ConvertPoint(pixelLocation, Map);

            if (FormsMap.Routes != null)
            {
                if (FormsMap.RouteClickedCommand != null)
                {
                    double maxMeters = MetersFromPixel(22, pixelLocation);
                    double nearestDistance = double.MaxValue;
                    TKRoute nearestRoute = null;

                    foreach (var route in FormsMap.Routes.Where(i => i.Selectable))
                    {
                        var internalItem = _routes.Single(i => i.Value.Overlay.Equals(route));
                        var distance = DistanceOfPoint(MKMapPoint.FromCoordinate(coordinate), internalItem.Key);

                        if (distance < nearestDistance)
                        {
                            nearestDistance = distance;
                            nearestRoute = internalItem.Value.Overlay;
                        }
                    }
                    if (nearestDistance <= maxMeters)
                    {
                        MapFunctions.RaiseRouteClicked(nearestRoute);
                        return;
                    }
                }
            }
            MapFunctions.RaiseMapClicked(coordinate.ToPosition());
        }
        /// <summary>
        /// When a long press was performed
        /// </summary>
        /// <param name="recognizer">The gesture recognizer</param>
        void OnMapLongPress(UILongPressGestureRecognizer recognizer)
        {
            if (recognizer.State != UIGestureRecognizerState.Began) return;

            var pixelLocation = recognizer.LocationInView(Map);
            var coordinate = Map.ConvertPoint(pixelLocation, Map);

            MapFunctions.RaiseMapLongPress(coordinate.ToPosition());
        }
        /// <summary>
        /// Get the view for the annotation
        /// </summary>
        /// <param name="mapView">The map</param>
        /// <param name="annotation">The annotation</param>
        /// <returns>The annotation view</returns>
        public virtual MKAnnotationView GetViewForAnnotation(MKMapView mapView, IMKAnnotation annotation)
        {
            var clusterAnnotation = annotation as CKCluster;
            var createDefaultClusterAnnotationView = false;

            MKAnnotationView annotationView;
            TKCustomMapAnnotation customAnnotation = null;

            if (clusterAnnotation == null)
            {
                customAnnotation = annotation as TKCustomMapAnnotation;
            }
            else
            {
                if (clusterAnnotation.Count > 1)
                {
                    var clusterPin = FormsMap.GetClusteredPin?.Invoke(null, clusterAnnotation.Annotations.OfType<TKCustomMapAnnotation>().Select(i => i.CustomPin));

                    if (clusterPin == null)
                    {
                        createDefaultClusterAnnotationView = true;
                    }
                    else
                    {
                        customAnnotation = new TKCustomMapAnnotation(clusterPin);
                    }
                }
                else
                {
                    customAnnotation = clusterAnnotation.FirstAnnotation as TKCustomMapAnnotation;
                }
            }

            if (createDefaultClusterAnnotationView)
            {
                annotationView = mapView.DequeueReusableAnnotation(AnnotationIdentifierDefaultClusterPin);
                if (annotationView == null)
                {
                    annotationView = new TKDefaultClusterAnnotationView(clusterAnnotation, AnnotationIdentifierDefaultClusterPin);
                }
                else
                {
                    annotationView.Annotation = clusterAnnotation;
                    (annotationView as TKDefaultClusterAnnotationView)?.Configure();
                }
            }
            else
            {

                if (customAnnotation == null) return null;

                if (customAnnotation.CustomPin.Image != null)
                {
                    annotationView = mapView.DequeueReusableAnnotation(AnnotationIdentifier);
                }
                else
                {
                    annotationView = mapView.DequeueReusableAnnotation(AnnotationIdentifierDefaultPin);
                }

                if (annotationView == null)
                {
                    if (customAnnotation.CustomPin.Image != null)
                    {
                        annotationView = new MKAnnotationView(customAnnotation, AnnotationIdentifier);
                        annotationView.Layer.AnchorPoint = new CGPoint(customAnnotation.CustomPin.Anchor.X, customAnnotation.CustomPin.Anchor.Y);
                    }
                    else
                    {
                        annotationView = new MKPinAnnotationView(customAnnotation, AnnotationIdentifierDefaultPin);
                    }
                }
                else
                {
                    annotationView.Annotation = customAnnotation;
                }
                annotationView.CanShowCallout = customAnnotation.CustomPin.ShowCallout;
                annotationView.Draggable = customAnnotation.CustomPin.IsDraggable;
                annotationView.Selected = _selectedAnnotation != null && customAnnotation.Equals(_selectedAnnotation);
                annotationView.Transform = CGAffineTransform.MakeRotation((float)customAnnotation.CustomPin.Rotation.ToRadian());

                SetAnnotationViewVisibility(annotationView, customAnnotation.CustomPin);
                UpdateImage(annotationView, customAnnotation.CustomPin);
                UpdateAccessoryView(customAnnotation.CustomPin, annotationView);
            }

            return annotationView;
        }
        /// <summary>
        /// Update the callout accessory view
        /// </summary>
        /// <param name="pin">Custom pin</param>
        /// <param name="view">Annotation view</param>
        void UpdateAccessoryView(TKCustomMapPin pin, MKAnnotationView view)
        {
            if (pin.IsCalloutClickable)
            {
                var button = new UIButton(UIButtonType.InfoLight);
                button.Frame = new CGRect(0, 0, 23, 23);
                button.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
                button.VerticalAlignment = UIControlContentVerticalAlignment.Center;
                view.RightCalloutAccessoryView = button;
            }
            else
            {
                view.RightCalloutAccessoryView = null;
            }
        }
        /// <summary>
        /// Creates the annotations
        /// </summary>
        void UpdatePins(bool firstUpdate = true)
        {
            if (FormsMap.IsClusteringEnabled)
            {
                _clusterMap.ClusterManager.RemoveAnnotations(_clusterMap.ClusterManager.Annotations);
            }
            else
            {
                Map.RemoveAnnotations(Map.Annotations);
            }

            if (FormsMap.Pins == null) return;

            foreach (var i in FormsMap.Pins)
            {
                i.PropertyChanged -= OnPinPropertyChanged;
                AddPin(i);
            }

            if (firstUpdate)
            {
                if (FormsMap.Pins is INotifyCollectionChanged observable)
                {
                    observable.CollectionChanged += OnCollectionChanged;
                }
            }
            MapFunctions.RaisePinsReady();
        }
        /// <summary>
        /// Creates the lines
        /// </summary>
        void UpdateLines(bool firstUpdate = true)
        {
            if (_lines.Any())
            {
                foreach (var line in _lines)
                {
                    line.Value.Overlay.PropertyChanged -= OnLinePropertyChanged;
                }

                Map?.RemoveOverlays(_lines.Select(i => i.Key).Cast<IMKOverlay>().ToArray());
                _lines.Clear();
            }

            if (FormsMap?.Polylines == null) return;

            foreach (var line in FormsMap.Polylines)
            {
                AddLine(line);
            }

            if (firstUpdate)
            {
                if (FormsMap.Polylines is INotifyCollectionChanged observable)
                {
                    observable.CollectionChanged += OnLineCollectionChanged;
                }
            }
        }
        /// <summary>
        /// Create the routes
        /// </summary>
        /// <param name="firstUpdate">First update of collection or not</param>
        void UpdateRoutes(bool firstUpdate = true)
        {
            _tempRouteList.Clear();

            if (_routes.Any())
            {
                foreach (var r in _routes.Where(i => i.Value != null))
                {
                    r.Value.Overlay.PropertyChanged -= OnRoutePropertyChanged;
                }

                Map?.RemoveOverlays(_routes.Select(i => i.Key).Cast<IMKOverlay>().ToArray());
                _routes.Clear();
            }
            if (FormsMap == null || FormsMap.Routes == null) return;

            foreach (var route in FormsMap.Routes)
            {
                AddRoute(route);
            }

            if (!firstUpdate) return;
            if (FormsMap.Routes is INotifyCollectionChanged observable)
            {
                observable.CollectionChanged += OnRouteCollectionChanged;
            }
        }
        /// <summary>
        /// When the collection of routes changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnRouteCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    foreach (TKRoute route in e.NewItems)
                    {
                        AddRoute(route);
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (TKRoute route in e.OldItems)
                    {
                        if (FormsMap.Routes.Contains(route)) continue;
                        route.PropertyChanged -= OnRoutePropertyChanged;

                        var (key, _) = _routes.SingleOrDefault(i => i.Value.Overlay.Equals(route));
                        if (key == null) continue;
                        Map.RemoveOverlay(key);
                        _routes.Remove(key);
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Reset:
                    UpdateRoutes(false);
                    break;
            }
        }
        /// <summary>
        /// Creates the circles on the map
        /// </summary>
        void UpdateCircles(bool firstUpdate = true)
        {
            if (_circles.Any())
            {
                foreach (var circle in _circles)
                {
                    circle.Value.Overlay.PropertyChanged -= OnCirclePropertyChanged;
                }
                Map.RemoveOverlays(_circles.Select(i => i.Key).Cast<IMKOverlay>().ToArray());
                _circles.Clear();
            }

            if (FormsMap.Circles == null) return;

            foreach (var circle in FormsMap.Circles)
            {
                AddCircle(circle);
            }

            if (!firstUpdate) return;
            if (FormsMap.Circles is INotifyCollectionChanged observable)
            {
                observable.CollectionChanged += OnCirclesCollectionChanged;
            }
        }
        /// <summary>
        /// Create the polygons
        /// </summary>
        /// <param name="firstUpdate">If the collection updates the first time</param>
        void UpdatePolygons(bool firstUpdate = true)
        {
            if (_polygons.Any())
            {
                foreach (var poly in _polygons)
                {
                    poly.Value.Overlay.PropertyChanged -= OnPolygonPropertyChanged;
                }
                Map.RemoveOverlays(_polygons.Select(i => i.Key).Cast<IMKOverlay>().ToArray());
                _polygons.Clear();
            }

            if (FormsMap.Polygons == null) return;

            foreach (var poly in FormsMap.Polygons)
            {
                AddPolygon(poly);
            }

            if (!firstUpdate) return;
            if (FormsMap.Polygons is INotifyCollectionChanged observable)
            {
                observable.CollectionChanged += OnPolygonsCollectionChanged;
            }
        }
        /// <summary>
        /// When the collection of polygons changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnPolygonsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    foreach (TKPolygon poly in e.NewItems)
                    {
                        AddPolygon(poly);
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (TKPolygon poly in e.OldItems)
                    {
                        if (FormsMap.Polygons.Contains(poly)) continue;
                        poly.PropertyChanged -= OnPolygonPropertyChanged;

                        var (key, _) = _polygons.SingleOrDefault(i => i.Value.Overlay.Equals(poly));
                        if (key == null) continue;
                        Map.RemoveOverlay(key);
                        _polygons.Remove(key);
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Reset:
                {
                    foreach (var poly in _polygons)
                    {
                        poly.Value.Overlay.PropertyChanged -= OnPolygonPropertyChanged;
                    }
                    UpdatePolygons(false);
                    break;
                }
            }
        }
        /// <summary>
        /// Adds a polygon to the map
        /// </summary>
        /// <param name="polygon">Polygon to add</param>
        void AddPolygon(TKPolygon polygon)
        {
            var mkPolygon = MKPolygon.FromCoordinates(polygon.Coordinates.Select(i => i.ToLocationCoordinate()).ToArray());
            _polygons.Add(mkPolygon, new TKOverlayItem<TKPolygon, MKPolygonRenderer>(polygon));
            Map.AddOverlay(mkPolygon);

            polygon.PropertyChanged += OnPolygonPropertyChanged;
        }
        /// <summary>
        /// When a property of a polygon changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnPolygonPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var poly = (TKPolygon)sender;

            if (poly == null) return;

            var (key, value) = _polygons.SingleOrDefault(i => i.Value.Overlay.Equals(poly));
            if (key == null) return;

            if (e.PropertyName != nameof(TKPolygon.Coordinates))
            {
                if (value.Renderer == null) return;
                switch (e.PropertyName)
                {
                    case nameof(TKPolygon.StrokeColor):
                        value.Renderer.StrokeColor = value.Overlay.StrokeColor.ToUIColor();
                        break;
                    case nameof(TKPolygon.Color):
                        value.Renderer.FillColor = value.Overlay.Color.ToUIColor();
                        break;
                    case nameof(TKPolygon.StrokeWidth):
                        value.Renderer.LineWidth = value.Overlay.StrokeWidth;
                        break;
                }
                return;
            }

            Map.RemoveOverlay(key);
            _polygons.Remove(key);

            var mkPolygon = MKPolygon.FromCoordinates(poly.Coordinates.Select(i => i.ToLocationCoordinate()).ToArray());
            _polygons.Add(mkPolygon, new TKOverlayItem<TKPolygon, MKPolygonRenderer>(poly));
            Map.AddOverlay(mkPolygon);
        }
        /// <summary>
        /// When the circles collection changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnCirclesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    foreach (TKCircle circle in e.NewItems)
                    {
                        AddCircle(circle);
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (TKCircle circle in e.OldItems)
                    {
                        if (FormsMap.Circles.Contains(circle)) continue;
                        circle.PropertyChanged -= OnCirclePropertyChanged;

                        var (key, _) = _circles.SingleOrDefault(i => i.Value.Overlay.Equals(circle));
                        if (key == null) continue;
                        Map.RemoveOverlay(key);
                        _circles.Remove(key);
                    }
                    // var o = new MKLocalSearchRequest();
                    break;
                }
                case NotifyCollectionChangedAction.Reset:
                    UpdateCircles(false);
                    break;
            }
        }
        /// <summary>
        /// When the route collection changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnLineCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    foreach (TKPolyline line in e.NewItems)
                    {
                        AddLine(line);
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (TKPolyline line in e.OldItems)
                    {
                        if (FormsMap.Polylines.Contains(line)) continue;
                        line.PropertyChanged -= OnLinePropertyChanged;

                        var (key, _) = _lines.SingleOrDefault(i => i.Value.Overlay.Equals(line));
                        if (key == null) continue;
                        Map.RemoveOverlay(key);
                        _lines.Remove(key);
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Reset:
                    UpdateLines(false);
                    break;
            }
        }
        /// <summary>
        /// Adds a pin
        /// </summary>
        /// <param name="pin">The pin to add</param>
        void AddPin(TKCustomMapPin pin)
        {
            var annotation = new TKCustomMapAnnotation(pin);

            if (FormsMap.IsClusteringEnabled)
            {
                _clusterMap.ClusterManager.AddAnnotation(annotation);
            }
            else
            {
                Map.AddAnnotation(annotation);
            }

            pin.PropertyChanged += OnPinPropertyChanged;
        }
        /// <summary>
        /// Adds a route
        /// </summary>
        /// <param name="line">The route to add</param>
        void AddLine(TKPolyline line)
        {
            var polyLine = MKPolyline.FromCoordinates(line.LineCoordinates.Select(i => i.ToLocationCoordinate()).ToArray());
            _lines.Add(polyLine, new TKOverlayItem<TKPolyline, MKPolylineRenderer>(line));
            Map.AddOverlay(polyLine);

            line.PropertyChanged += OnLinePropertyChanged;
        }
        /// <summary>
        /// Adds a route to the map
        /// </summary>
        /// <param name="route">The route to add</param>
        void AddRoute(TKRoute route)
        {
            _tempRouteList.Add(route);

            var req = new MKDirectionsRequest
            {
                Source = new MKMapItem(
                    new MKPlacemark(route.Source.ToLocationCoordinate(), 
                        new MKPlacemarkAddress())), 
                Destination = new MKMapItem(
                    new MKPlacemark(route.Destination.ToLocationCoordinate(), 
                        new MKPlacemarkAddress())), 
                TransportType = route.TravelMode.ToTransportType()
            };

            var directions = new MKDirections(req);
            directions.CalculateDirections((r, e) =>
            {
                if (FormsMap == null || Map == null || !_tempRouteList.Contains(route)) return;

                if (e == null)
                {
                    var nativeRoute = r.Routes.First();

                    SetRouteData(route, nativeRoute);

                    _routes.Add(nativeRoute.Polyline, new TKOverlayItem<TKRoute, MKPolylineRenderer>(route));
                    Map.AddOverlay(nativeRoute.Polyline);

                    route.PropertyChanged += OnRoutePropertyChanged;

                    MapFunctions.RaiseRouteCalculationFinished(route);
                }
                else
                {
                    var routeCalculationError = new TKRouteCalculationError(route, e.ToString());
                    MapFunctions.RaiseRouteCalculationFailed(routeCalculationError);
                }
            });
        }
        /// <summary>
        /// When a property of a route changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnRoutePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var route = (TKRoute)sender;

            if (route == null) return;

            var (key, value) = _routes.SingleOrDefault(i => i.Value.Overlay.Equals(route));
            if (key == null) return;

            if (e.PropertyName != nameof(TKRoute.TravelMode) &&
                e.PropertyName != nameof(TKRoute.Source) &&
                e.PropertyName != nameof(TKRoute.Destination))
            {
                if (value.Renderer == null) return;
                switch (e.PropertyName)
                {
                    case nameof(TKRoute.Color):
                        value.Renderer.FillColor = value.Overlay.Color.ToUIColor();
                        value.Renderer.StrokeColor = value.Overlay.Color.ToUIColor();
                        break;
                    case nameof(TKPolyline.LineWidth):
                        value.Renderer.LineWidth = value.Overlay.LineWidth;
                        break;
                }
                return;
            }

            value.Overlay.PropertyChanged -= OnRoutePropertyChanged;

            Map.RemoveOverlay(key);
            _routes.Remove(key);

            AddRoute(route);
        }
        /// <summary>
        /// Adds a circle to the map
        /// </summary>
        /// <param name="circle">The circle to add</param>
        void AddCircle(TKCircle circle)
        {
            var mkCircle = MKCircle.Circle(circle.Center.ToLocationCoordinate(), circle.Radius);

            _circles.Add(mkCircle, new TKOverlayItem<TKCircle, MKCircleRenderer>(circle));
            Map.AddOverlay(mkCircle);

            circle.PropertyChanged += OnCirclePropertyChanged;
        }
        /// <summary>
        /// When a property of a circle changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnCirclePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var circle = (TKCircle)sender;

            if (circle == null) return;

            var (key, value) = _circles.SingleOrDefault(i => i.Value.Overlay.Equals(circle));
            if (key == null) return;

            if (e.PropertyName != nameof(TKCircle.Center) &&
                e.PropertyName != nameof(TKCircle.Radius))
            {
                if (value.Renderer == null) return;
                switch (e.PropertyName)
                {
                    case nameof(TKCircle.Color):
                        value.Renderer.FillColor = value.Overlay.Color.ToUIColor();
                        break;
                    case nameof(TKCircle.StrokeColor):
                        value.Renderer.StrokeColor = value.Overlay.StrokeColor.ToUIColor();
                        break;
                    case nameof(TKCircle.StrokeWidth):
                        value.Renderer.LineWidth = value.Overlay.StrokeWidth;
                        break;
                }
                return;
            }

            Map.RemoveOverlay(key);
            _circles.Remove(key);

            var mkCircle = MKCircle.Circle(circle.Center.ToLocationCoordinate(), circle.Radius);
            _circles.Add(mkCircle, new TKOverlayItem<TKCircle, MKCircleRenderer>(circle));
            Map.AddOverlay(mkCircle);
        }
        /// <summary>
        /// When a property of the route changes, re-add the <see cref="MKPolyline"/>
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnLinePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var line = (TKPolyline)sender;

            if (line == null) return;

            var (key, value) = _lines.SingleOrDefault(i => i.Value.Overlay.Equals(line));
            if (key == null) return;

            if (e.PropertyName != nameof(TKPolyline.LineCoordinates))
            {
                if (value.Renderer == null) return;
                switch (e.PropertyName)
                {
                    case nameof(TKOverlay.Color):
                        value.Renderer.FillColor = value.Overlay.Color.ToUIColor();
                        value.Renderer.StrokeColor = value.Overlay.Color.ToUIColor();
                        break;
                    case nameof(TKPolyline.LineWidth):
                        value.Renderer.LineWidth = value.Overlay.LineWidth;
                        break;
                }
                return;
            }

            Map.RemoveOverlay(key);
            _lines.Remove(key);

            var polyLine = MKPolyline.FromCoordinates(line.LineCoordinates.Select(i => i.ToLocationCoordinate()).ToArray());
            _lines.Add(polyLine, new TKOverlayItem<TKPolyline, MKPolylineRenderer>(line));
            Map.AddOverlay(polyLine);
        }
        /// <summary>
        /// When a property of the pin changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        void OnPinPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TKCustomMapPin.Title) ||
                e.PropertyName == nameof(TKCustomMapPin.Subtitle) ||
                (e.PropertyName == nameof(TKCustomMapPin.Position) && _isDragging))
                return;

            var formsPin = (TKCustomMapPin)sender;
            var annotation = GetCustomAnnotation(formsPin);

            if (annotation == null) return;

            var annotationView = GetViewByAnnotation(annotation);

            if (annotationView == null) return;

            switch (e.PropertyName)
            {
                case nameof(TKCustomMapPin.Image):
                    UpdateImage(annotationView, formsPin);
                    break;
                case nameof(TKCustomMapPin.DefaultPinColor):
                    UpdateImage(annotationView, formsPin);
                    break;
                case nameof(TKCustomMapPin.IsDraggable):
                    annotationView.Draggable = formsPin.IsDraggable;
                    break;
                case nameof(TKCustomMapPin.IsVisible):
                    SetAnnotationViewVisibility(annotationView, formsPin);
                    break;
                case nameof(TKCustomMapPin.Position):
                    annotationView.Annotation.SetCoordinate(formsPin.Position.ToLocationCoordinate());
                    annotation.SetCoordinateInternal(formsPin.Position.ToLocationCoordinate(), true);
                    break;
                case nameof(TKCustomMapPin.ShowCallout):
                    annotationView.CanShowCallout = formsPin.ShowCallout;
                    break;
                case nameof(TKCustomMapPin.Anchor):
                    if (formsPin.Image != null)
                    {
                        annotationView.Layer.AnchorPoint = new CGPoint(formsPin.Anchor.X, formsPin.Anchor.Y);
                    }
                    break;
                case nameof(TKCustomMapPin.Rotation):
                    annotationView.Transform = CGAffineTransform.MakeRotation((float)formsPin.Rotation);
                    break;
                case nameof(TKCustomMapPin.IsCalloutClickable):
                    UpdateAccessoryView(formsPin, annotationView);
                    break;
            }
        }
        /// <summary>
        /// Sets the route data
        /// </summary>
        /// <param name="route">PCL route</param>
        /// <param name="nativeRoute">Native route</param>
        void SetRouteData(TKRoute route, MKRoute nativeRoute)
        {
            var routeFunctions = (IRouteFunctions)route;
            var steps = new TKRouteStep[nativeRoute.Steps.Count()];

            for (var i = 0; i < steps.Length; i++)
            {
                steps[i] = new TKRouteStep();
                var stepFunction = (IRouteStepFunctions)steps[i];
                var nativeStep = nativeRoute.Steps.ElementAt(i);

                stepFunction.SetInstructions(nativeStep.Instructions);
                stepFunction.SetDistance(nativeStep.Distance);
            }

            routeFunctions.SetSteps(steps);
            routeFunctions.SetDistance(nativeRoute.Distance);
            routeFunctions.SetTravelTime(nativeRoute.ExpectedTravelTime);

            var region = MKCoordinateRegion.FromMapRect(Map.MapRectThatFits(nativeRoute.Polyline.BoundingMapRect, new UIEdgeInsets(25, 25, 25, 25)));

            routeFunctions.SetBounds(new MapSpan(region.Center.ToPosition(), region.Span.LatitudeDelta, region.Span.LongitudeDelta));
            routeFunctions.SetIsCalculated(true);
        }
        /// <summary>
        /// Set the visibility of an annotation view
        /// </summary>
        /// <param name="annotationView">The annotation view</param>
        /// <param name="pin">The forms pin</param>
        void SetAnnotationViewVisibility(MKAnnotationView annotationView, TKCustomMapPin pin)
        {
            annotationView.Hidden = !pin.IsVisible;
            annotationView.UserInteractionEnabled = pin.IsVisible;
            annotationView.Enabled = pin.IsVisible;
        }
        /// <summary>
        /// Set the image of the annotation view
        /// </summary>
        /// <param name="annotationView">The annotation view</param>
        /// <param name="pin">The forms pin</param>
        async void UpdateImage(MKAnnotationView annotationView, TKCustomMapPin pin)
        {
            if (pin.Image != null)
            {
                // If this is the case, we need to get a whole new annotation view. 
                if (annotationView.GetType() == typeof(MKPinAnnotationView))
                {

                    if (FormsMap.IsClusteringEnabled)
                    {
                        _clusterMap.ClusterManager.RemoveAnnotation(GetCustomAnnotation(annotationView));
                        _clusterMap.ClusterManager.AddAnnotation(new TKCustomMapAnnotation(pin));
                    }
                    else
                    {
                        Map.RemoveAnnotation(GetCustomAnnotation(annotationView));
                        Map.AddAnnotation(new TKCustomMapAnnotation(pin));
                    }
                    return;
                }
                var image = await pin.Image.ToImage();
                Device.BeginInvokeOnMainThread(() =>
                {
                    annotationView.Image = image;
                });
            }
            else
            {
                if (annotationView is MKPinAnnotationView pinAnnotationView)
                {
                    pinAnnotationView.AnimatesDrop = AnimateOnPinDrop;

                    var pinTintColorAvailable = pinAnnotationView.RespondsToSelector(new Selector("pinTintColor"));

                    if (!pinTintColorAvailable)
                    {
                        return;
                    }

                    pinAnnotationView.PinTintColor = pin.DefaultPinColor != Color.Default 
                        ? pin.DefaultPinColor.ToUIColor() 
                        : UIColor.Red;
                }
                else
                {
                    if (FormsMap.IsClusteringEnabled)
                    {
                        _clusterMap.ClusterManager.RemoveAnnotation(GetCustomAnnotation(annotationView));
                        _clusterMap.ClusterManager.AddAnnotation(new TKCustomMapAnnotation(pin));
                    }
                    else
                    {
                        Map.RemoveAnnotation(GetCustomAnnotation(annotationView));
                        Map.AddAnnotation(new TKCustomMapAnnotation(pin));
                    }
                }
            }
        }
        /// <summary>
        /// Updates the tiles and adds or removes the overlay
        /// </summary>
        void UpdateTileOptions()
        {
            if (Map == null) return;

            if (_tileOverlay != null)
            {
                Map.RemoveOverlay(_tileOverlay);
                _tileOverlay = null;
            }

            if (FormsMap == null || FormsMap.TilesUrlOptions == null) return;
            _tileOverlay = new MKTileOverlay(
                FormsMap.TilesUrlOptions.TilesUrl
                    .Replace("{0}", "{x}")
                    .Replace("{1}", "{y}")
                    .Replace("{2}", "{z}"))
            {
                TileSize = new CGSize(
                    FormsMap.TilesUrlOptions.TileWidth,
                    FormsMap.TilesUrlOptions.TileHeight),
                MinimumZ = FormsMap.TilesUrlOptions.MinimumZoomLevel,
                MaximumZ = FormsMap.TilesUrlOptions.MaximumZoomLevel,
                CanReplaceMapContent = true
            };



            Map.AddOverlay(_tileOverlay);
        }
        /// <summary>
        /// Sets the selected pin
        /// </summary>
        void SetSelectedPin()
        {
            if (_selectedAnnotation is TKCustomMapAnnotation customMapAnnotation)
            {
                if (customMapAnnotation.CustomPin.Equals(FormsMap.SelectedPin)) return;

                var annotationView = GetViewByAnnotation(customMapAnnotation);
                if (annotationView != null)
                {
                    annotationView.Selected = false;
                    Map.DeselectAnnotation(annotationView.Annotation, true);
                }

                _selectedAnnotation = null;
            }

            if (FormsMap.SelectedPin == null) return;
            {
                var selectedAnnotation = GetCustomAnnotation(FormsMap.SelectedPin);

                if (selectedAnnotation == null) return;
                var annotationView = GetViewByAnnotation(selectedAnnotation);
                _selectedAnnotation = selectedAnnotation;
                if (annotationView != null)
                {
                    Map.SelectAnnotation(annotationView.Annotation, true);
                }
                MapFunctions.RaisePinSelected(FormsMap.SelectedPin);
            }
        }
        /// <summary>
        /// Sets traffic enabled on the map
        /// </summary>
        void UpdateShowTraffic()
        {
            if (FormsMap == null || Map == null) return;

            var showsTrafficAvailable = Map.RespondsToSelector(new Selector("showsTraffic"));
            if (!showsTrafficAvailable) return;

            Map.ShowsTraffic = FormsMap.ShowTraffic;
        }
        /// <summary>
        /// Updates the map region when changed
        /// </summary>
        void UpdateMapRegion()
        {
            if (FormsMap?.MapRegion == null) return;

            if (Map.GetCurrentMapRegion().Equals(FormsMap.MapRegion)) return;
            MoveToMapRegion(FormsMap.MapRegion, FormsMap.IsRegionChangeAnimated);
        }
        /// <summary>
        /// Updates clustering
        /// </summary>
        void UpdateIsClusteringEnabled()
        {
            if (FormsMap == null || Map == null) return;

            if (FormsMap.IsClusteringEnabled)
            {
                if (_clusterMap == null)
                {
                    _clusterMap = new TKClusterMap(Map);
                }

                Map.RemoveAnnotations(Map.Annotations);
                foreach (var pin in FormsMap.Pins)
                {
                    AddPin(pin);
                }
                _clusterMap.ClusterManager.UpdateClusters();
            }
            else
            {
                _clusterMap.ClusterManager.RemoveAnnotations(_clusterMap.ClusterManager.Annotations);
                foreach (var pin in FormsMap.Pins)
                {
                    AddPin(pin);
                }
                _clusterMap.Dispose();
                _clusterMap = null;
            }
        }
        /// <summary>
        /// Updates the map type
        /// </summary>
        void UpdateMapType()
        {
            if (FormsMap == null || Map == null) return;
            switch (FormsMap.MapType)
            {
                case MapType.Hybrid:
                    Map.MapType = MKMapType.Hybrid;
                    break;
                case MapType.Satellite:
                    Map.MapType = MKMapType.Satellite;
                    break;
                case MapType.Street:
                    Map.MapType = MKMapType.Standard;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        /// <summary>
        /// Sets whether the user should be shown
        /// </summary>
        void UpdateIsShowingUser()
        {
            if (FormsMap == null || Map == null) return;

            if (FormsMap.IsShowingUser && UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                _locationManager = new CLLocationManager();
                _locationManager.RequestWhenInUseAuthorization();
            }

            Map.ShowsUserLocation = FormsMap.IsShowingUser;
        }
        /// <summary>
        /// Update ScrollEnabled
        /// </summary>
        void UpdateHasScrollEnabled()
        {
            if (FormsMap == null || Map == null) return;

            Map.ScrollEnabled = FormsMap.HasScrollEnabled;
        }
        /// <summary>
        /// Update ZoomEnabled
        /// </summary>
        void UpdateHasZoomEnabled()
        {
            if (FormsMap == null || Map == null) return;

            Map.ZoomEnabled = FormsMap.HasZoomEnabled;
        }

        /// <summary>
        /// Calculates the closest distance of a point to a polyline
        /// </summary>
        /// <param name="pt">The point</param>
        /// <param name="poly">The polyline</param>
        /// <returns>The closes distance</returns>
        double DistanceOfPoint(MKMapPoint pt, MKPolyline poly)
        {
            double distance = float.MaxValue;
            for (var n = 0; n < poly.PointCount - 1; n++)
            {

                var ptA = poly.Points[n];
                var ptB = poly.Points[n + 1];

                var xDelta = ptB.X - ptA.X;
                var yDelta = ptB.Y - ptA.Y;

                if (Math.Abs(xDelta - default(double)) <= 0 && Math.Abs(yDelta - default(double)) <= 0)
                {

                    // Points must not be equal
                    continue;
                }

                var u = ((pt.X - ptA.X) * xDelta + (pt.Y - ptA.Y) * yDelta) / (xDelta * xDelta + yDelta * yDelta);
                MKMapPoint ptClosest;
                if (u < 0.0)
                {

                    ptClosest = ptA;
                }
                else if (u > 1.0)
                {

                    ptClosest = ptB;
                }
                else
                {

                    ptClosest = new MKMapPoint(ptA.X + u * xDelta, ptA.Y + u * yDelta);
                }

                distance = Math.Min(distance, MKGeometry.MetersBetweenMapPoints(ptClosest, pt));
            }

            return distance;
        }
        /// <summary>
        /// Returns the meters between two points
        /// </summary>
        /// <param name="px">X in pixels</param>
        /// <param name="pt">Position</param>
        /// <returns>Distance in meters</returns>
        double MetersFromPixel(int px, CGPoint pt)
        {
            var ptB = new CGPoint(pt.X + px, pt.Y);

            var coordA = Map.ConvertPoint(pt, Map);
            var coordB = Map.ConvertPoint(ptB, Map);

            return MKGeometry.MetersBetweenMapPoints(MKMapPoint.FromCoordinate(coordA), MKMapPoint.FromCoordinate(coordB));
        }
        /// <summary>
        /// Convert a <see cref="MKCoordinateRegion"/> to <see cref="MKMapRect"/>
        /// http://stackoverflow.com/questions/9270268/convert-mkcoordinateregion-to-mkmaprect
        /// </summary>
        /// <param name="region">Region to convert</param>
        /// <returns>The map rect</returns>
        MKMapRect RegionToRect(MKCoordinateRegion region)
        {
            var a = MKMapPoint.FromCoordinate(
                new CLLocationCoordinate2D(
                    region.Center.Latitude + region.Span.LatitudeDelta / 2,
                    region.Center.Longitude - region.Span.LongitudeDelta / 2));

            var b = MKMapPoint.FromCoordinate(
                new CLLocationCoordinate2D(
                    region.Center.Latitude - region.Span.LatitudeDelta / 2,
                    region.Center.Longitude + region.Span.LongitudeDelta / 2));

            return new MKMapRect(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
        }
        /// <summary>
        /// Unregisters all collections
        /// </summary>
        void UnregisterCollections(TKCustomMap map)
        {
            UnregisterCollection(map.Pins, OnCollectionChanged, OnPinPropertyChanged);
            UnregisterCollection(map.Routes, OnRouteCollectionChanged, OnRoutePropertyChanged);
            UnregisterCollection(map.Polylines, OnLineCollectionChanged, OnLinePropertyChanged);
            UnregisterCollection(map.Circles, OnCirclesCollectionChanged, OnCirclePropertyChanged);
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
            if (collection is null) return;

            if (collection is INotifyCollectionChanged observable)
            {
                observable.CollectionChanged -= observableHandler;
            }
            foreach (INotifyPropertyChanged obj in collection)
            {
                obj.PropertyChanged -= propertyHandler;
            }
        }
        ///<inheritdoc/>
        public async Task<byte[]> GetSnapshot()
        {
            UIImage img = null;
            await Task.Factory.StartNew(() =>
            {
                BeginInvokeOnMainThread(() =>
                {
                    UIGraphics.BeginImageContextWithOptions(Frame.Size, false, 0.0f);
                    Layer.RenderInContext(UIGraphics.GetCurrentContext());

                    img = UIGraphics.GetImageFromCurrentImageContext();
                    UIGraphics.EndImageContext();
                });
            });
            return img.AsPNG().ToArray();
        }
        /// <inheritdoc/>
        public void FitMapRegionToPositions(IEnumerable<Position> positions, bool animate = false, int padding = 0)
        {
            if (Map == null) return;

            var zoomRect = positions
                .Select(position => MKMapPoint.FromCoordinate(position.ToLocationCoordinate()))
                .Select(point => new MKMapRect(point.X, point.Y, 0.1, 0.1))
                .Aggregate(MKMapRect.Null, MKMapRect.Union);

            Map.SetVisibleMapRect(zoomRect, new UIEdgeInsets(padding, padding, padding, padding), animate);
        }
        /// <inheritdoc/>
        public void MoveToMapRegion(MapSpan region, bool animate)
        {
            if (Map == null) return;

            var coordinateRegion = MKCoordinateRegion.FromDistance(
                region.Center.ToLocationCoordinate(),
                region.Radius.Meters * 2,
                region.Radius.Meters * 2);

            Map.SetRegion(coordinateRegion, animate);
        }
        /// <inheritdoc/>
        public void FitToMapRegions(IEnumerable<MapSpan> regions, bool animate = false, int padding = 0)
        {
            if (Map == null) return;

            var rect = regions.Aggregate(MKMapRect.Null, 
                (current, region) => MKMapRect.Union(current, 
                    RegionToRect(MKCoordinateRegion.FromDistance(region.Center.ToLocationCoordinate(), 
                        region.Radius.Meters * 2, 
                        region.Radius.Meters * 2))));
            Map.SetVisibleMapRect(rect, new UIEdgeInsets(padding, padding, padding, padding), animate);
        }
        /// <inheritdoc/>
        public void ShowCallout(TKCustomMapPin pin)
        {
            if (Map == null) return;

            var annotation = GetCustomAnnotation(pin);

            if (FormsMap.IsClusteringEnabled)
            {
                _clusterMap.ClusterManager.SelectAnnotation(annotation, true);
            }
            else
            {
                Map.SelectAnnotation(annotation, true);
            }
        }
        /// <inheritdoc/>
        public void HideCallout(TKCustomMapPin pin)
        {
            if (Map == null) return;

            var annotation = GetCustomAnnotation(pin);

            if (FormsMap.IsClusteringEnabled)
            {
                _clusterMap.ClusterManager.DeselectAnnotation(annotation, true);
            }
            else
            {
                Map.DeselectAnnotation(annotation, true);
            }
        }
        /// <inheritdoc />
        public IEnumerable<Position> ScreenLocationsToGeocoordinates(params Point[] screenLocations)
        {
            if (Map == null)
                throw new InvalidOperationException("Map not initialized");

            return screenLocations.Select(i => Map.ConvertPoint(i.ToCGPoint(), Map).ToPosition());
        }
        /// <summary>
        /// Returns the <see cref="TKCustomMapPin"/> by the native <see cref="IMKAnnotation"/>
        /// </summary>
        /// <param name="annotation">The annotation to search with</param>
        /// <returns>The forms pin</returns>
        protected TKCustomMapPin GetPinByAnnotation(IMKAnnotation annotation)
        {
            if (!FormsMap.IsClusteringEnabled) 
                return (annotation as TKCustomMapAnnotation)?.CustomPin;
            var customAnnotation = (CKCluster)FromObject(annotation); ;
            return customAnnotation.Annotations.Count() > 1
                ? FormsMap.GetClusteredPin?.Invoke(null, customAnnotation.Annotations.OfType<TKCustomMapAnnotation>().Select(i => i.CustomPin)) 
                : customAnnotation.Annotations.OfType<TKCustomMapAnnotation>().FirstOrDefault()?.CustomPin;

        }
        /// <summary>
        /// Remove all annotations before disposing
        /// </summary>
        /// <param name="disposing">disposing</param>
        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;

            _disposed = true;

            if (disposing)
            {
                DisposeRenderers();
                if (Map != null)
                {
                    _clusterMap?.ClusterManager?.RemoveAnnotations(_clusterMap.ClusterManager.Annotations);
                    Map.RemoveAnnotations(Map.Annotations);

                    Map.GetViewForAnnotation = null;
                    Map.OverlayRenderer = null;
                    Map.DidSelectAnnotationView -= OnDidSelectAnnotationView;
                    Map.RegionChanged -= OnMapRegionChanged;
                    Map.DidUpdateUserLocation -= OnDidUpdateUserLocation;
                    Map.ChangedDragState -= OnChangedDragState;
                    Map.CalloutAccessoryControlTapped -= OnMapCalloutAccessoryControlTapped;

                    Map.RemoveGestureRecognizer(_longPressGestureRecognizer);
                    Map.RemoveGestureRecognizer(_tapGestureRecognizer);
                    Map.RemoveGestureRecognizer(_doubleTapGestureRecognizer);
                    _longPressGestureRecognizer.Dispose();
                    _tapGestureRecognizer.Dispose();
                    _doubleTapGestureRecognizer.Dispose();

                    Map.Dispose();
                    _clusterMap?.Dispose();

                }
                if (FormsMap != null)
                {
                    FormsMap.PropertyChanged -= OnMapPropertyChanged;
                    UnregisterCollections(FormsMap);
                }
            }
            
            base.Dispose(disposing); 
            
            void DisposeRenderers()
            {
                foreach (var route in _routes.Select(x => x.Value)) 
                    DisposeTkOverlayItem(route);

                foreach (var line in _lines.Select(x => x.Value)) 
                    DisposeTkOverlayItem(line);

                foreach (var circle in _circles.Select(x => x.Value)) 
                    DisposeTkOverlayItem(circle);

                foreach (var polygon in _polygons.Select(x => x.Value)) 
                    DisposeTkOverlayItem(polygon);
            
                _routes.Clear();
                _lines.Clear();
                _circles.Clear();
                _polygons.Clear();

                DisposeTkOverlayItem(_polygonRenderer);
                DisposeTkOverlayItem(_lineRenderer);
                DisposeTkOverlayItem(_circleRenderer);
                DisposeTkOverlayItem(_routeRenderer);

                void DisposeTkOverlayItem<TRenderer,TOverlay>(TKOverlayItem<TOverlay,TRenderer> overlay)
                    where TRenderer: MKOverlayPathRenderer
                    where TOverlay : TKOverlay
                {
                    if (overlay is null) return;
                    overlay.Renderer?.Dispose();
                    overlay.Renderer = null;
                    overlay.Overlay = null;
                }
            }
        }

        TKCustomMapAnnotation GetCustomAnnotation(MKAnnotationView view)
        {
            if (!FormsMap.IsClusteringEnabled) return view.Annotation as TKCustomMapAnnotation;
            var cluster = Runtime.GetNSObject(view.Annotation.Handle) as CKCluster;

            if (cluster?.Annotations.Count() != 1) return null;

            return cluster.Annotations.First() as TKCustomMapAnnotation;

        }
        TKCustomMapAnnotation GetCustomAnnotation(TKCustomMapPin pin) =>
            FormsMap.IsClusteringEnabled 
                ? _clusterMap.ClusterManager.Annotations.OfType<TKCustomMapAnnotation>().SingleOrDefault(i => i.CustomPin.Equals(pin)) 
                : Map.Annotations.OfType<TKCustomMapAnnotation>().SingleOrDefault(i => i.CustomPin.Equals(pin));

        MKAnnotationView GetViewByAnnotation(TKCustomMapAnnotation annotation) =>
            FormsMap.IsClusteringEnabled 
                ? Map.ViewForAnnotation(Map.Annotations.GetCluster(annotation))
                : Map.ViewForAnnotation(annotation);
    }
}
