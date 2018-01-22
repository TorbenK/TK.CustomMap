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
using System.Threading;

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

        MKMapView Map
        {
            get { return Control as MKMapView; }
        }
        TKCustomMap FormsMap
        {
            get { return Element as TKCustomMap; }
        }
        IMapFunctions MapFunctions
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
                UnregisterCollections((TKCustomMap)e.OldElement);

                Map.RemoveGestureRecognizer(_longPressGestureRecognizer);
                Map.RemoveGestureRecognizer(_tapGestureRecognizer);
                Map.RemoveGestureRecognizer(_doubleTapGestureRecognizer);
                _longPressGestureRecognizer.Dispose();
                _tapGestureRecognizer.Dispose();
                _doubleTapGestureRecognizer.Dispose();
            }

            if (e.NewElement != null)
            {
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
        }

        /// <summary>
        /// Get the overlay renderer
        /// </summary>
        /// <param name="mapView">The <see cref="MKMapView"/></param>
        /// <param name="overlay">The overlay to render</param>
        /// <returns>The overlay renderer</returns>
        MKOverlayRenderer GetOverlayRenderer(MKMapView mapView, IMKOverlay overlay)
        {

            var polyline = overlay as MKPolyline;
            if (polyline != null)
            {
                // check if this polyline is a route
                var isRoute = _routes.ContainsKey(polyline);
                if (!isRoute)
                {
                    // not a route, check if it is a line
                    var line = _lines[polyline];

                    if (line.Renderer == null)
                    {
                        line.Renderer = new MKPolylineRenderer(polyline);
                    }
                    line.Renderer.FillColor = line.Overlay.Color.ToUIColor();
                    line.Renderer.LineWidth = line.Overlay.LineWidth;
                    line.Renderer.StrokeColor = line.Overlay.Color.ToUIColor();

                    // return renderer for the line
                    return line.Renderer;
                }
                else
                {
                    var route = _routes[polyline];
                    if (route.Renderer == null)
                    {
                        route.Renderer = new MKPolylineRenderer(polyline);
                    }
                    route.Renderer.FillColor = route.Overlay.Color.ToUIColor();
                    route.Renderer.LineWidth = route.Overlay.LineWidth;
                    route.Renderer.StrokeColor = route.Overlay.Color.ToUIColor();
                    return route.Renderer;
                }
            }

            var mkCircle = overlay as MKCircle;
            if (mkCircle != null)
            {
                var circle = _circles[mkCircle];

                if (circle.Renderer == null)
                {
                    circle.Renderer = new MKCircleRenderer(mkCircle);
                }
                circle.Renderer.FillColor = circle.Overlay.Color.ToUIColor();
                circle.Renderer.StrokeColor = circle.Overlay.StrokeColor.ToUIColor();
                circle.Renderer.LineWidth = circle.Overlay.StrokeWidth;
                return circle.Renderer;
            }

            var mkPolygon = overlay as MKPolygon;
            if (mkPolygon != null)
            {
                var polygon = _polygons[mkPolygon];

                if (polygon.Renderer == null)
                {
                    polygon.Renderer = new MKPolygonRenderer(mkPolygon);
                }

                polygon.Renderer.FillColor = polygon.Overlay.Color.ToUIColor();
                polygon.Renderer.StrokeColor = polygon.Overlay.StrokeColor.ToUIColor();
                polygon.Renderer.LineWidth = polygon.Overlay.StrokeWidth;
                return polygon.Renderer;
            }

            if (overlay is MKTileOverlay)
            {
                if (_tileOverlayRenderer != null)
                {
                    _tileOverlayRenderer.Dispose();
                }

                return (_tileOverlayRenderer = new MKTileOverlayRenderer(_tileOverlay));
            }

            return null;
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
                    _clusterMap.ClusterManager.RemoveAnnotations(annotationsToRemove.ToArray());
                }
                else
                {
                    Map.RemoveAnnotations(annotationsToRemove.ToArray());
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
                    var ckCluster = e.AnnotationView.Annotation as CKCluster;
                    annotation.SetCoordinateInternal(ckCluster.Coordinate, true);
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
            bool createDefaultClusterAnnotatioView = false;

            MKAnnotationView annotationView = null;
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
                        createDefaultClusterAnnotatioView = true;
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

            if (createDefaultClusterAnnotatioView)
            {
                annotationView = mapView.DequeueReusableAnnotation(AnnotationIdentifierDefaultClusterPin);
                if(annotationView == null)
                {
                    annotationView = new TKDefaultClusterAnnotationView(clusterAnnotation, AnnotationIdentifierDefaultClusterPin);
                }
                else
                {
                    annotationView.Annotation = clusterAnnotation;
                    (annotationView as TKDefaultClusterAnnotationView).Configure();
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
                var observAble = FormsMap.Pins as INotifyCollectionChanged;
                if (observAble != null)
                {
                    observAble.CollectionChanged += OnCollectionChanged;
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
                if (Map != null)
                    Map.RemoveOverlays(_lines.Select(i => i.Key).ToArray());
                _lines.Clear();
            }

            if (FormsMap == null || FormsMap.Polylines == null) return;

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
                if (Map != null)
                    Map.RemoveOverlays(_routes.Select(i => i.Key).ToArray());
                _routes.Clear();
            }
            if (FormsMap == null || FormsMap.Routes == null) return;

            foreach (var route in FormsMap.Routes)
            {
                AddRoute(route);
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
                        route.PropertyChanged -= OnRoutePropertyChanged;

                        var item = _routes.SingleOrDefault(i => i.Value.Overlay.Equals(route));
                        if (item.Key != null)
                        {
                            Map.RemoveOverlay(item.Key);
                            _routes.Remove(item.Key);
                        }
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                UpdateRoutes(false);
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
                Map.RemoveOverlays(_circles.Select(i => i.Key).ToArray());
                _circles.Clear();
            }

            if (FormsMap.Circles == null) return;

            foreach (var circle in FormsMap.Circles)
            {
                AddCircle(circle);
            }
            if (firstUpdate)
            {
                var observAble = FormsMap.Circles as INotifyCollectionChanged;
                if (observAble != null)
                {
                    observAble.CollectionChanged += OnCirclesCollectionChanged;
                }
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
                Map.RemoveOverlays(_polygons.Select(i => i.Key).ToArray());
                _polygons.Clear();
            }

            if (FormsMap.Polygons == null) return;

            foreach (var poly in FormsMap.Polygons)
            {
                AddPolygon(poly);
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
        /// <summary>
        /// When the collection of polygons changed
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
                        poly.PropertyChanged -= OnPolygonPropertyChanged;

                        var item = _polygons.SingleOrDefault(i => i.Value.Overlay.Equals(poly));
                        if (item.Key != null)
                        {
                            Map.RemoveOverlay(item.Key);
                            _polygons.Remove(item.Key);
                        }
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var poly in _polygons)
                {
                    poly.Value.Overlay.PropertyChanged -= OnPolygonPropertyChanged;
                }
                UpdatePolygons(false);
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

            var item = _polygons.SingleOrDefault(i => i.Value.Overlay.Equals(poly));
            if (item.Key == null) return;

            if (e.PropertyName != nameof(TKPolygon.Coordinates))
            {
                if (item.Value.Renderer == null) return;
                if (e.PropertyName == nameof(TKPolygon.StrokeColor))
                {
                    item.Value.Renderer.StrokeColor = item.Value.Overlay.StrokeColor.ToUIColor();
                }
                else if (e.PropertyName == nameof(TKPolygon.Color))
                {
                    item.Value.Renderer.FillColor = item.Value.Overlay.Color.ToUIColor();
                }
                else if (e.PropertyName == nameof(TKPolygon.StrokeWidth))
                {
                    item.Value.Renderer.LineWidth = item.Value.Overlay.StrokeWidth;
                }
                return;
            }

            Map.RemoveOverlay(item.Key);
            _polygons.Remove(item.Key);

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
                        circle.PropertyChanged -= OnCirclePropertyChanged;

                        var item = _circles.SingleOrDefault(i => i.Value.Overlay.Equals(circle));
                        if (item.Key != null)
                        {
                            Map.RemoveOverlay(item.Key);
                            _circles.Remove(item.Key);
                        }
                    }
                }
                MKLocalSearchRequest o = new MKLocalSearchRequest();

            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                UpdateCircles(false);
            }
        }
        /// <summary>
        /// When the route collection changed
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
                        line.PropertyChanged -= OnLinePropertyChanged;

                        var item = _lines.SingleOrDefault(i => i.Value.Overlay.Equals(line));
                        if (item.Key != null)
                        {
                            Map.RemoveOverlay(item.Key);
                            _lines.Remove(item.Key);
                        }
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                UpdateLines(false);
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

            MKDirectionsRequest req = new MKDirectionsRequest();
            req.Source = new MKMapItem(new MKPlacemark(route.Source.ToLocationCoordinate(), new MKPlacemarkAddress()));
            req.Destination = new MKMapItem(new MKPlacemark(route.Destination.ToLocationCoordinate(), new MKPlacemarkAddress()));
            req.TransportType = route.TravelMode.ToTransportType();

            MKDirections directions = new MKDirections(req);
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

            var item = _routes.SingleOrDefault(i => i.Value.Overlay.Equals(route));
            if (item.Key == null) return;

            if (e.PropertyName != nameof(TKRoute.TravelMode) &&
                e.PropertyName != nameof(TKRoute.Source) &&
                e.PropertyName != nameof(TKRoute.Destination))
            {
                if (item.Value.Renderer == null) return;
                if (e.PropertyName == nameof(TKRoute.Color))
                {
                    item.Value.Renderer.FillColor = item.Value.Overlay.Color.ToUIColor();
                    item.Value.Renderer.StrokeColor = item.Value.Overlay.Color.ToUIColor();
                }
                else if (e.PropertyName == nameof(TKPolyline.LineWidth))
                {
                    item.Value.Renderer.LineWidth = item.Value.Overlay.LineWidth;
                }
                return;
            }

            item.Value.Overlay.PropertyChanged -= OnRoutePropertyChanged;

            Map.RemoveOverlay(item.Key);
            _routes.Remove(item.Key);

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

            var item = _circles.SingleOrDefault(i => i.Value.Overlay.Equals(circle));
            if (item.Key == null) return;

            if (e.PropertyName != nameof(TKCircle.Center) &&
                e.PropertyName != nameof(TKCircle.Radius))
            {
                if (item.Value.Renderer == null) return;
                if (e.PropertyName == nameof(TKCircle.Color))
                {
                    item.Value.Renderer.FillColor = item.Value.Overlay.Color.ToUIColor();
                }
                else if (e.PropertyName == nameof(TKCircle.StrokeColor))
                {
                    item.Value.Renderer.StrokeColor = item.Value.Overlay.StrokeColor.ToUIColor();
                }
                else if (e.PropertyName == nameof(TKCircle.StrokeWidth))
                {
                    item.Value.Renderer.LineWidth = item.Value.Overlay.StrokeWidth;
                }
                return;
            }

            Map.RemoveOverlay(item.Key);
            _circles.Remove(item.Key);

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

            var item = _lines.SingleOrDefault(i => i.Value.Overlay.Equals(line));
            if (item.Key == null) return;

            if (e.PropertyName != nameof(TKPolyline.LineCoordinates))
            {
                if (item.Value.Renderer == null) return;
                if (e.PropertyName == nameof(TKOverlay.Color))
                {
                    item.Value.Renderer.FillColor = item.Value.Overlay.Color.ToUIColor();
                    item.Value.Renderer.StrokeColor = item.Value.Overlay.Color.ToUIColor();
                }
                else if (e.PropertyName == nameof(TKPolyline.LineWidth))
                {
                    item.Value.Renderer.LineWidth = item.Value.Overlay.LineWidth;
                }
                return;
            }

            Map.RemoveOverlay(item.Key);
            _lines.Remove(item.Key);

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

            MKAnnotationView annotationView = GetViewByAnnotation(annotation);

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

            for (int i = 0; i < steps.Length; i++)
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
                UIImage image = await pin.Image.ToImage();
                Device.BeginInvokeOnMainThread(() =>
                {
                    annotationView.Image = image;
                });
            }
            else
            {
                var pinAnnotationView = annotationView as MKPinAnnotationView;
                if (pinAnnotationView != null)
                {
                    pinAnnotationView.AnimatesDrop = AnimateOnPinDrop;

                    var pinTintColorAvailable = pinAnnotationView.RespondsToSelector(new Selector("pinTintColor"));

                    if (!pinTintColorAvailable)
                    {
                        return;
                    }

                    if (pin.DefaultPinColor != Color.Default)
                    {
                        pinAnnotationView.PinTintColor = pin.DefaultPinColor.ToUIColor();
                    }
                    else
                    {
                        pinAnnotationView.PinTintColor = UIColor.Red;
                    }
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

            if (FormsMap != null && FormsMap.TilesUrlOptions != null)
            {
                _tileOverlay = new MKTileOverlay(
                    FormsMap.TilesUrlOptions.TilesUrl
                        .Replace("{0}", "{x}")
                        .Replace("{1}", "{y}")
                        .Replace("{2}", "{z}"));

                _tileOverlay.TileSize = new CGSize(
                    FormsMap.TilesUrlOptions.TileWidth,
                    FormsMap.TilesUrlOptions.TileHeight);

                _tileOverlay.MinimumZ = FormsMap.TilesUrlOptions.MinimumZoomLevel;
                _tileOverlay.MaximumZ = FormsMap.TilesUrlOptions.MaximumZoomLevel;

                _tileOverlay.CanReplaceMapContent = true;
                Map.AddOverlay(_tileOverlay);
            }
        }
        /// <summary>
        /// Sets the selected pin
        /// </summary>
        void SetSelectedPin()
        {
            var customAnnotion = _selectedAnnotation as TKCustomMapAnnotation;

            if (customAnnotion != null)
            {
                if (customAnnotion.CustomPin.Equals(FormsMap.SelectedPin)) return;

                var annotationView = GetViewByAnnotation(customAnnotion);
                if (annotationView != null)
                {
                    annotationView.Selected = false;
                    Map.DeselectAnnotation(annotationView.Annotation, true);
                }

                _selectedAnnotation = null;
            }
            if (FormsMap.SelectedPin != null)
            {
                var selectedAnnotation = GetCustomAnnotation(FormsMap.SelectedPin);

                if (selectedAnnotation != null)
                {
                    var annotationView = GetViewByAnnotation(selectedAnnotation);
                    _selectedAnnotation = selectedAnnotation;
                    if (annotationView != null)
                    {
                        Map.SelectAnnotation(annotationView.Annotation, true);
                    }
                    MapFunctions.RaisePinSelected(FormsMap.SelectedPin);
                }
            }
        }
        /// <summary>
        /// Sets traffic enabled on the map
        /// </summary>
        void UpdateShowTraffic()
        {
            if (FormsMap == null || Map == null) return;

            var showsTrafficAvailable = Map.RespondsToSelector(new Selector("showsTraffic"));
            if (!showsTrafficAvailable)
            {
                return;
            }

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
            }
        }
        /// <summary>
        /// Sets whether the user should be shown
        /// </summary>
        void UpdateIsShowingUser()
        {
            if (FormsMap == null || Map == null) return;

            if(FormsMap.IsShowingUser && UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
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
            for (int n = 0; n < poly.PointCount - 1; n++)
            {

                MKMapPoint ptA = poly.Points[n];
                MKMapPoint ptB = poly.Points[n + 1];

                double xDelta = ptB.X - ptA.X;
                double yDelta = ptB.Y - ptA.Y;

                if (xDelta == 0.0 && yDelta == 0.0)
                {

                    // Points must not be equal
                    continue;
                }

                double u = ((pt.X - ptA.X) * xDelta + (pt.Y - ptA.Y) * yDelta) / (xDelta * xDelta + yDelta * yDelta);
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
            CGPoint ptB = new CGPoint(pt.X + px, pt.Y);

            CLLocationCoordinate2D coordA = Map.ConvertPoint(pt, Map);
            CLLocationCoordinate2D coordB = Map.ConvertPoint(ptB, Map);

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
            MKMapPoint a = MKMapPoint.FromCoordinate(
                new CLLocationCoordinate2D(
                    region.Center.Latitude + region.Span.LatitudeDelta / 2,
                    region.Center.Longitude - region.Span.LongitudeDelta / 2));

            MKMapPoint b = MKMapPoint.FromCoordinate(
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

            MKMapRect zoomRect = MKMapRect.Null;

            foreach (var position in positions)
            {
                MKMapPoint point = MKMapPoint.FromCoordinate(position.ToLocationCoordinate());
                MKMapRect pointRect = new MKMapRect(point.X, point.Y, 0.1, 0.1);
                zoomRect = MKMapRect.Union(zoomRect, pointRect);
            }
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

            MKMapRect rect = MKMapRect.Null;
            foreach (var region in regions)
            {
                rect = MKMapRect.Union(
                    rect,
                    RegionToRect(
                        MKCoordinateRegion.FromDistance(
                            region.Center.ToLocationCoordinate(),
                            region.Radius.Meters * 2,
                            region.Radius.Meters * 2)));
            }
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
            if (FormsMap.IsClusteringEnabled)
            {
                var customAnnotation = annotation as CKCluster;
                if (customAnnotation.Annotations.Count() > 1)
                {
                    return FormsMap.GetClusteredPin?.Invoke(null, customAnnotation.Annotations.OfType<TKCustomMapAnnotation>().Select(i => i.CustomPin));
                }
                else
                {
                    return customAnnotation.Annotations.OfType<TKCustomMapAnnotation>().FirstOrDefault()?.CustomPin;
                }
            }

            return (annotation as TKCustomMapAnnotation)?.CustomPin;
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
        }
        TKCustomMapAnnotation GetCustomAnnotation(MKAnnotationView view)
        {
            if (FormsMap.IsClusteringEnabled)
            {
                var cluster = view.Annotation as CKCluster;

                if (cluster?.Annotations.Count() != 1) return null;

                return cluster.Annotations.First() as TKCustomMapAnnotation;
            }
            else
            {
                return view.Annotation as TKCustomMapAnnotation;
            }
        }
        TKCustomMapAnnotation GetCustomAnnotation(TKCustomMapPin pin)
        {
            if (FormsMap.IsClusteringEnabled)
            {
                return _clusterMap.ClusterManager.Annotations.OfType<TKCustomMapAnnotation>().SingleOrDefault(i => i.CustomPin.Equals(pin));
            }
            else
            {
                return Map.Annotations.OfType<TKCustomMapAnnotation>().SingleOrDefault(i => i.CustomPin.Equals(pin));
            }
        }
        MKAnnotationView GetViewByAnnotation(TKCustomMapAnnotation annotation)
        {
            if (FormsMap.IsClusteringEnabled)
            {
                return Map.ViewForAnnotation(Map.Annotations.GetCluster(annotation));
            }
            else
            {
                return Map.ViewForAnnotation(annotation);
            }
        }

    }
}
