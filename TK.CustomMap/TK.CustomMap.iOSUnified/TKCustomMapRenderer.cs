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
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.iOS;
using Xamarin.Forms.Platform.iOS;
using System.Collections;

[assembly: ExportRenderer(typeof(TKCustomMap), typeof(TKCustomMapRenderer))]

namespace TK.CustomMap.iOSUnified
{
    /// <summary>
    /// iOS Renderer of <see cref="TK.CustomMap.TKCustomMap"/>
    /// </summary>
    [Preserve(AllMembers = true)]
    public class TKCustomMapRenderer : MapRenderer, IRendererFunctions
    {
        private bool _isLoaded;

        private const double MercatorRadius = 85445659.44705395;

        private const string AnnotationIdentifier = "TKCustomAnnotation";
        private const string AnnotationIdentifierDefaultPin = "TKCustomAnnotationDefaultPin";

        private readonly List<TKRoute> _tempRouteList = new List<TKRoute>();

        private readonly Dictionary<MKPolyline, TKOverlayItem<TKRoute, MKPolylineRenderer>> _routes = new Dictionary<MKPolyline, TKOverlayItem<TKRoute, MKPolylineRenderer>>();
        private readonly Dictionary<MKPolyline, TKOverlayItem<TKPolyline, MKPolylineRenderer>> _lines = new Dictionary<MKPolyline, TKOverlayItem<TKPolyline, MKPolylineRenderer>>();
        private readonly Dictionary<MKCircle, TKOverlayItem<TKCircle, MKCircleRenderer>> _circles = new Dictionary<MKCircle, TKOverlayItem<TKCircle, MKCircleRenderer>>();
        private readonly Dictionary<MKPolygon, TKOverlayItem<TKPolygon, MKPolygonRenderer>> _polygons = new Dictionary<MKPolygon, TKOverlayItem<TKPolygon, MKPolygonRenderer>>();

        private bool _isDragging;
        private IMKAnnotation _selectedAnnotation;
        private MKTileOverlay _tileOverlay;
        private MKTileOverlayRenderer _tileOverlayRenderer;
        private UIGestureRecognizer _longPressGestureRecognizer;
        private UIGestureRecognizer _tapGestureRecognizer;
        private UIGestureRecognizer _doubleTapGestureRecognizer;

        private MKMapView Map
        {
            get { return this.Control as MKMapView; }
        }
        private TKCustomMap FormsMap
        {
            get { return this.Element as TKCustomMap; }
        }
        private IMapFunctions MapFunctions
        {
            get { return this.Element as IMapFunctions; }
        }
        /// <summary>
        /// Dummy function to avoid linker.
        /// </summary>
        [Preserve]
        public static void InitMapRenderer()
        {
            var temp = DateTime.Now;
        }
        /// <inheritdoc/>
        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);

            if(e.OldElement != null && this.Map != null)
            {
                e.OldElement.PropertyChanged -= OnMapPropertyChanged;

                this._isLoaded = false;

                this.Map.MapLoaded -= MapLoaded;
                this.Map.GetViewForAnnotation = null;
                this.Map.OverlayRenderer = null;
                this.Map.DidSelectAnnotationView -= OnDidSelectAnnotationView;
                this.Map.RegionChanged -= OnMapRegionChanged;
                this.Map.DidUpdateUserLocation -= OnDidUpdateUserLocation;
                this.Map.ChangedDragState -= OnChangedDragState;
                this.Map.CalloutAccessoryControlTapped -= OnMapCalloutAccessoryControlTapped;
                this.UnregisterCollections((TKCustomMap)e.OldElement);

                this.Map.RemoveGestureRecognizer(this._longPressGestureRecognizer);
                this.Map.RemoveGestureRecognizer(this._tapGestureRecognizer);
                this.Map.RemoveGestureRecognizer(this._doubleTapGestureRecognizer);
                this._longPressGestureRecognizer.Dispose();
                this._tapGestureRecognizer.Dispose();
                this._doubleTapGestureRecognizer.Dispose();
            }

            if (e.NewElement != null)
            {
                this.MapFunctions.SetRenderer(this);

                this.Map.GetViewForAnnotation = this.GetViewForAnnotation;
                this.Map.OverlayRenderer = this.GetOverlayRenderer;
                this.Map.DidSelectAnnotationView += OnDidSelectAnnotationView;
                this.Map.RegionChanged += OnMapRegionChanged;
                this.Map.DidUpdateUserLocation += OnDidUpdateUserLocation;
                this.Map.ChangedDragState += OnChangedDragState;
                this.Map.CalloutAccessoryControlTapped += OnMapCalloutAccessoryControlTapped;

                this.Map.AddGestureRecognizer((this._longPressGestureRecognizer = new UILongPressGestureRecognizer(this.OnMapLongPress)));

                this._doubleTapGestureRecognizer = new UITapGestureRecognizer() { NumberOfTapsRequired = 2 };

                this._tapGestureRecognizer = new UITapGestureRecognizer(this.OnMapClicked);
                this._tapGestureRecognizer.RequireGestureRecognizerToFail(this._doubleTapGestureRecognizer);
                this._tapGestureRecognizer.ShouldReceiveTouch = (recognizer, touch) => !(touch.View is MKAnnotationView);

                this.Map.AddGestureRecognizer(this._tapGestureRecognizer);
                this.Map.AddGestureRecognizer(this._doubleTapGestureRecognizer);

                this.Map.MapLoaded += MapLoaded;
            }
        }
        /// <summary>
        /// Initially set all data when map is loaded
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void MapLoaded(object sender, EventArgs e)
        {
            if (this._isLoaded) return;

            this.UpdateTileOptions();
            this.SetMapCenter();
            this.UpdatePins();
            this.UpdateRoutes();
            this.UpdateLines();
            this.UpdateCircles();
            this.UpdatePolygons();
            this.UpdateShowTraffic();
            this.FormsMap.PropertyChanged += OnMapPropertyChanged;

            this._isLoaded = true;
        }
        /// <summary>
        /// Get the overlay renderer
        /// </summary>
        /// <param name="mapView">The <see cref="MKMapView"/></param>
        /// <param name="overlay">The overlay to render</param>
        /// <returns>The overlay renderer</returns>
        private MKOverlayRenderer GetOverlayRenderer(MKMapView mapView, IMKOverlay overlay)
        {

            var polyline = overlay as MKPolyline;
            if (polyline != null)
            {
                // check if this polyline is a route
                var isRoute = this._routes.ContainsKey(polyline);
                if (!isRoute)
                {
                    // not a route, check if it is a line
                    var line = this._lines[polyline];

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
                    var route = this._routes[polyline];
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
                var circle = this._circles[mkCircle];

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
                var polygon = this._polygons[mkPolygon];

                if (polygon.Renderer == null)
                {
                    polygon.Renderer = new MKPolygonRenderer(mkPolygon);
                }
                
                polygon.Renderer.FillColor = polygon.Overlay.Color.ToUIColor();
                polygon.Renderer.StrokeColor = polygon.Overlay.StrokeColor.ToUIColor();
                polygon.Renderer.LineWidth = polygon.Overlay.StrokeWidth;
                return polygon.Renderer;
            }

            if(overlay is MKTileOverlay)
            {
                if (this._tileOverlayRenderer != null)
                {
                    this._tileOverlayRenderer.Dispose();
                }

                return (this._tileOverlayRenderer = new MKTileOverlayRenderer(this._tileOverlay));
            }

            return null;
        }
        /// <summary>
        /// When the user location changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnDidUpdateUserLocation(object sender, MKUserLocationEventArgs e)
        {
            if (e.UserLocation == null || this.FormsMap == null || this.FormsMap.UserLocationChangedCommand == null) return;

            var newPosition = e.UserLocation.Location.Coordinate.ToPosition();
            this.MapFunctions.RaiseUserLocationChanged(newPosition);
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
                this.UpdatePins();
            }
            else if (e.PropertyName == TKCustomMap.SelectedPinProperty.PropertyName)
            {
                this.SetSelectedPin();
            }
            else if (e.PropertyName == TKCustomMap.MapCenterProperty.PropertyName)
            {
                this.SetMapCenter();
            }
            else if (e.PropertyName == TKCustomMap.PolylinesProperty.PropertyName)
            {
                this.UpdateLines();
            }
            else if (e.PropertyName == TKCustomMap.CalloutClickedCommandProperty.PropertyName)
            {
                this.UpdatePins(false);
            }
            else if(e.PropertyName == TKCustomMap.PolygonsProperty.PropertyName)
            {
                this.UpdatePolygons();
            }
            else if (e.PropertyName == TKCustomMap.RoutesProperty.PropertyName)
            {
                this.UpdateRoutes();
            }
            else if(e.PropertyName == TKCustomMap.TilesUrlOptionsProperty.PropertyName)
            {
                this.UpdateTileOptions();
            }
            else if(e.PropertyName == TKCustomMap.ShowTrafficProperty.PropertyName)
            {
                this.UpdateShowTraffic();
            }
            else if(e.PropertyName == TKCustomMap.MapRegionProperty.PropertyName)
            {
                this.UpdateMapRegion();
            }
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
                foreach(TKCustomMapPin pin in e.NewItems)
                {
                    this.AddPin(pin);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (TKCustomMapPin pin in e.OldItems)
                {
                    if (!this.FormsMap.CustomPins.Contains(pin))
                    {
                        if (this.FormsMap.SelectedPin != null && this.FormsMap.SelectedPin.Equals(pin))
                        {
                            this.FormsMap.SelectedPin = null;
                        }

                        var annotation = this.Map.Annotations
                            .OfType<TKCustomMapAnnotation>()
                            .SingleOrDefault(i => i.CustomPin.Equals(pin));

                        if (annotation != null)
                        {
                            annotation.CustomPin.PropertyChanged -= OnPinPropertyChanged;
                            this.Map.RemoveAnnotation(annotation);
                        }
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var annotation in this.Map.Annotations.OfType<TKCustomMapAnnotation>())
                {
                    annotation.CustomPin.PropertyChanged -= OnPinPropertyChanged;
                }
                this.UpdatePins(false);
            }
        }
        /// <summary>
        /// When the accessory control of a callout gets tapped
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnMapCalloutAccessoryControlTapped(object sender, MKMapViewAccessoryTappedEventArgs e)
        {
            this.MapFunctions.RaiseCalloutClicked(this.GetPinByAnnotation(e.View.Annotation));
        } 
        /// <summary>
        /// When the drag state changed
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnChangedDragState(object sender, MKMapViewDragStateEventArgs e)
        {
            var annotation = e.AnnotationView.Annotation as TKCustomMapAnnotation;
            if (annotation == null) return;

            if (e.NewState == MKAnnotationViewDragState.Starting)
            {
                this._isDragging = true;
            }
            else if (e.NewState == MKAnnotationViewDragState.Dragging)
            {
                annotation.CustomPin.Position = e.AnnotationView.Annotation.Coordinate.ToPosition();
            }
            else if (e.NewState == MKAnnotationViewDragState.Ending || e.NewState == MKAnnotationViewDragState.Canceling)
            {
                e.AnnotationView.DragState = MKAnnotationViewDragState.None;
                this._isDragging = false;
                this.MapFunctions.RaisePinDragEnd(annotation.CustomPin);
            }
        }
        /// <summary>
        /// When the camera region changed
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnMapRegionChanged(object sender, MKMapViewChangeEventArgs e)
        {
            this.FormsMap.MapCenter = this.Map.CenterCoordinate.ToPosition();
        }
        /// <summary>
        /// When an annotation view got selected
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        public virtual void OnDidSelectAnnotationView(object sender, MKAnnotationViewEventArgs e)
        {
            var pin = e.View.Annotation as TKCustomMapAnnotation;
            if(pin == null) return;

            this._selectedAnnotation = e.View.Annotation;
            this.FormsMap.SelectedPin = pin.CustomPin;
            
            this.MapFunctions.RaisePinSelected(pin.CustomPin);
        }
        /// <summary>
        /// When a tap was perfomed on the map
        /// </summary>
        /// <param name="recognizer">The gesture recognizer</param>
        private void OnMapClicked(UITapGestureRecognizer recognizer)
        {
            if (recognizer.State != UIGestureRecognizerState.Ended) return;

            var pixelLocation = recognizer.LocationInView(this.Map);
            var coordinate = this.Map.ConvertPoint(pixelLocation, this.Map);

            if (this.FormsMap.Routes != null)
            {
                if (this.FormsMap.RouteClickedCommand != null)
                {
                    double maxMeters = this.MetersFromPixel(22, pixelLocation);
                    double nearestDistance = double.MaxValue;
                    TKRoute nearestRoute = null;

                    foreach (var route in this.FormsMap.Routes.Where(i => i.Selectable))
                    {
                        var internalItem = this._routes.Single(i => i.Value.Overlay.Equals(route));
                        var distance = this.DistanceOfPoint(MKMapPoint.FromCoordinate(coordinate), internalItem.Key);

                        if (distance < nearestDistance)
                        {
                            nearestDistance = distance;
                            nearestRoute = internalItem.Value.Overlay;
                        }
                    }
                    if (nearestDistance <= maxMeters)
                    {
                        this.MapFunctions.RaiseRouteClicked(nearestRoute);
                        return;
                    }
                }
            }
            this.MapFunctions.RaiseMapClicked(coordinate.ToPosition());
        }
        /// <summary>
        /// When a long press was performed
        /// </summary>
        /// <param name="recognizer">The gesture recognizer</param>
        private void OnMapLongPress(UILongPressGestureRecognizer recognizer)
        {
            if (recognizer.State != UIGestureRecognizerState.Began) return;

            var pixelLocation = recognizer.LocationInView(this.Map);
            var coordinate = this.Map.ConvertPoint(pixelLocation, this.Map);

            this.MapFunctions.RaiseMapLongPress(coordinate.ToPosition());
        }
        /// <summary>
        /// Get the view for the annotation
        /// </summary>
        /// <param name="mapView">The map</param>
        /// <param name="annotation">The annotation</param>
        /// <returns>The annotation view</returns>
        public virtual MKAnnotationView GetViewForAnnotation(MKMapView mapView, IMKAnnotation annotation)
        {
            var customAnnotation = annotation as TKCustomMapAnnotation;
            
            if (customAnnotation == null) return null;

            MKAnnotationView annotationView;
            if(customAnnotation.CustomPin.Image != null)
                annotationView = mapView.DequeueReusableAnnotation(AnnotationIdentifier);
            else
                annotationView = mapView.DequeueReusableAnnotation(AnnotationIdentifierDefaultPin);
            
            if (annotationView == null)
            {
                if (customAnnotation.CustomPin.Image != null)
                {
                    annotationView = new MKAnnotationView(customAnnotation, AnnotationIdentifier);
                    annotationView.Layer.AnchorPoint = new CGPoint(customAnnotation.CustomPin.Anchor.X, customAnnotation.CustomPin.Anchor.Y);
                }
                else
                    annotationView = new MKPinAnnotationView(customAnnotation, AnnotationIdentifierDefaultPin);
            }
            else 
            {
                annotationView.Annotation = customAnnotation;
            }
            customAnnotation.Point(annotationView);
            annotationView.CanShowCallout = customAnnotation.CustomPin.ShowCallout;
            annotationView.Draggable = customAnnotation.CustomPin.IsDraggable;
            annotationView.Selected = this._selectedAnnotation != null && customAnnotation.Equals(this._selectedAnnotation);
            annotationView.Transform = CGAffineTransform.MakeRotation((float)customAnnotation.CustomPin.Rotation.ToRadian());

            this.SetAnnotationViewVisibility(annotationView, customAnnotation.CustomPin);
            this.UpdateImage(annotationView, customAnnotation.CustomPin);
            this.UpdateAccessoryView(customAnnotation.CustomPin, annotationView);
            
            return annotationView;
        }
        /// <summary>
        /// Update the callout accessory view
        /// </summary>
        /// <param name="pin">Custom pin</param>
        /// <param name="view">Annotation view</param>
        private void UpdateAccessoryView(TKCustomMapPin pin, MKAnnotationView view)
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
        private void UpdatePins(bool firstUpdate = true)
        {
            this.Map.RemoveAnnotations(this.Map.Annotations);

            if (this.FormsMap.CustomPins == null) return;

            foreach (var i in FormsMap.CustomPins)
            {
                i.PropertyChanged -= OnPinPropertyChanged;
                this.AddPin(i);
            }

            if (firstUpdate)
            {
                var observAble = this.FormsMap.CustomPins as INotifyCollectionChanged;
                if (observAble != null)
                {
                    observAble.CollectionChanged += OnCollectionChanged;
                }
            }
            this.MapFunctions.RaisePinsReady();
        }
        /// <summary>
        /// Creates the lines
        /// </summary>
        private void UpdateLines(bool firstUpdate = true)
        {
            if (this._lines.Any())
            {
                foreach(var line in this._lines)
                {
                    line.Value.Overlay.PropertyChanged -= OnLinePropertyChanged;
                }
                if(this.Map != null)
                    this.Map.RemoveOverlays(this._lines.Select(i => i.Key).ToArray());
                this._lines.Clear();
            }

            if (this.FormsMap == null || this.FormsMap.Polylines == null) return;

            foreach (var line in this.FormsMap.Polylines)
            {
                this.AddLine(line);
            }

            if (firstUpdate)
            {
                var observAble = this.FormsMap.Polylines as INotifyCollectionChanged;
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
        private void UpdateRoutes(bool firstUpdate = true)
        {
            this._tempRouteList.Clear();

            if (this._routes.Any())
            {
                foreach(var r in this._routes.Where(i => i.Value != null))
                {
                    r.Value.Overlay.PropertyChanged -= OnRoutePropertyChanged;
                }
				if (this.Map != null)
                	this.Map.RemoveOverlays(this._routes.Select(i => i.Key).ToArray());
                this._routes.Clear();
            }
			if (this.FormsMap == null || this.FormsMap.Routes == null) return;

            foreach (var route in this.FormsMap.Routes)
            {
                this.AddRoute(route);
            }

            if (firstUpdate)
            {
                var observAble = this.FormsMap.Routes as INotifyCollectionChanged;
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
                    this.AddRoute(route);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (TKRoute route in e.OldItems)
                {
                    if (!this.FormsMap.Routes.Contains(route))
                    {
                        route.PropertyChanged -= OnRoutePropertyChanged;

                        var item = this._routes.SingleOrDefault(i => i.Value.Overlay.Equals(route));
                        if (item.Key != null)
                        {
                            this.Map.RemoveOverlay(item.Key);
                            this._routes.Remove(item.Key);
                        }
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.UpdateRoutes(false);
            }
        }
        /// <summary>
        /// Creates the circles on the map
        /// </summary>
        private void UpdateCircles(bool firstUpdate = true)
        {
            if (this._circles.Any())
            {
                foreach(var circle in this._circles)
                {
                    circle.Value.Overlay.PropertyChanged -= OnCirclePropertyChanged;
                }
                this.Map.RemoveOverlays(this._circles.Select(i => i.Key).ToArray());
                this._circles.Clear();
            }

            if (this.FormsMap.Circles == null) return;

            foreach (var circle in this.FormsMap.Circles)
            {
                this.AddCircle(circle);
            }
            if (firstUpdate)
            {
                var observAble = this.FormsMap.Circles as INotifyCollectionChanged;
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
        private void UpdatePolygons(bool firstUpdate = true)
        {
            if (this._polygons.Any())
            {
                foreach(var poly in this._polygons)
                {
                    poly.Value.Overlay.PropertyChanged -= OnPolygonPropertyChanged;
                }
                this.Map.RemoveOverlays(this._polygons.Select(i => i.Key).ToArray());
                this._polygons.Clear();
            }

            if (this.FormsMap.Polygons == null) return;

            foreach (var poly in this.FormsMap.Polygons)
            {
                this.AddPolygon(poly);
            }
            if (firstUpdate)
            {
                var observAble = this.FormsMap.Polygons as INotifyCollectionChanged;
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
        private void OnPolygonsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (TKPolygon poly in e.NewItems)
                {
                    this.AddPolygon(poly);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (TKPolygon poly in e.OldItems)
                {
                    if (!this.FormsMap.Polygons.Contains(poly))
                    {
                        poly.PropertyChanged -= OnPolygonPropertyChanged;

                        var item = this._polygons.SingleOrDefault(i => i.Value.Overlay.Equals(poly));
                        if (item.Key != null)
                        {
                            this.Map.RemoveOverlay(item.Key);
                            this._polygons.Remove(item.Key);
                        }
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var poly in this._polygons)
                {
                    poly.Value.Overlay.PropertyChanged -= OnPolygonPropertyChanged;
                }
                this.UpdatePolygons(false);
            }
        }
        /// <summary>
        /// Adds a polygon to the map
        /// </summary>
        /// <param name="polygon">Polygon to add</param>
        private void AddPolygon(TKPolygon polygon)
        {
            var mkPolygon = MKPolygon.FromCoordinates(polygon.Coordinates.Select(i => i.ToLocationCoordinate()).ToArray());
            this._polygons.Add(mkPolygon, new TKOverlayItem<TKPolygon,MKPolygonRenderer>(polygon));
            this.Map.AddOverlay(mkPolygon);

            polygon.PropertyChanged += OnPolygonPropertyChanged;
        }
        /// <summary>
        /// When a property of a polygon changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnPolygonPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var poly = (TKPolygon)sender;

            if (poly == null) return;

            var item = this._polygons.SingleOrDefault(i => i.Value.Overlay.Equals(poly));
            if (item.Key == null) return;

            if (e.PropertyName != TKPolygon.CoordinatesPropertyName)
            {
                if (e.PropertyName == TKPolygon.StrokeColorPropertyName)
                {
                    item.Value.Renderer.StrokeColor = item.Value.Overlay.StrokeColor.ToUIColor();
                }
                else if (e.PropertyName == TKPolygon.ColorPropertyName)
                {
                    item.Value.Renderer.FillColor = item.Value.Overlay.Color.ToUIColor();
                }
                else if (e.PropertyName == TKPolygon.StrokeWidthPropertyName)
                {
                    item.Value.Renderer.LineWidth = item.Value.Overlay.StrokeWidth;
                }
                return;
            }

            this.Map.RemoveOverlay(item.Key);
            this._polygons.Remove(item.Key);

            var mkPolygon = MKPolygon.FromCoordinates(poly.Coordinates.Select(i => i.ToLocationCoordinate()).ToArray());
            this._polygons.Add(mkPolygon, new TKOverlayItem<TKPolygon,MKPolygonRenderer>(poly));
            this.Map.AddOverlay(mkPolygon);
        }
        /// <summary>
        /// When the circles collection changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnCirclesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (TKCircle circle in e.NewItems)
                {
                    this.AddCircle(circle);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (TKCircle circle in e.OldItems)
                {
                    if (!this.FormsMap.Circles.Contains(circle))
                    {
                        circle.PropertyChanged -= OnCirclePropertyChanged;

                        var item = this._circles.SingleOrDefault(i => i.Value.Overlay.Equals(circle));
                        if (item.Key != null)
                        {
                            this.Map.RemoveOverlay(item.Key);
                            this._circles.Remove(item.Key);
                        }
                    }
                }
                MKLocalSearchRequest o = new MKLocalSearchRequest();
                
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.UpdateCircles(false);
            }
        }
        /// <summary>
        /// When the route collection changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnLineCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (TKPolyline line in e.NewItems)
                {
                    this.AddLine(line);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (TKPolyline line in e.OldItems)
                {
                    if (!this.FormsMap.Polylines.Contains(line))
                    {
                        line.PropertyChanged -= OnLinePropertyChanged;

                        var item = this._lines.SingleOrDefault(i => i.Value.Overlay.Equals(line));
                        if (item.Key != null)
                        {
                            this.Map.RemoveOverlay(item.Key);
                            this._lines.Remove(item.Key);
                        }
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.UpdateLines(false);
            }
        }
        /// <summary>
        /// Adds a pin
        /// </summary>
        /// <param name="pin">The pin to add</param>
        private void AddPin(TKCustomMapPin pin)
        {
            var annotation = new TKCustomMapAnnotation(pin);
            this.Map.AddAnnotation(annotation);

            pin.PropertyChanged += OnPinPropertyChanged;
        }
        /// <summary>
        /// Adds a route
        /// </summary>
        /// <param name="line">The route to add</param>
        private void AddLine(TKPolyline line)
        {
            var polyLine = MKPolyline.FromCoordinates(line.LineCoordinates.Select(i => i.ToLocationCoordinate()).ToArray());
            this._lines.Add(polyLine, new TKOverlayItem<TKPolyline,MKPolylineRenderer>(line));
            this.Map.AddOverlay(polyLine);

            line.PropertyChanged += OnLinePropertyChanged;
        }
        /// <summary>
        /// Adds a route to the map
        /// </summary>
        /// <param name="route">The route to add</param>
        private void AddRoute(TKRoute route)
        {
            this._tempRouteList.Add(route);

            MKDirectionsRequest req = new MKDirectionsRequest();
            req.Source = new MKMapItem(new MKPlacemark(route.Source.ToLocationCoordinate(), new MKPlacemarkAddress()));
            req.Destination = new MKMapItem(new MKPlacemark(route.Destination.ToLocationCoordinate(), new MKPlacemarkAddress()));
            req.TransportType = route.TravelMode.ToTransportType();
            
            MKDirections directions = new MKDirections(req);
            directions.CalculateDirections((r, e) => 
            {
				if (this.FormsMap == null || this.Map == null || !this._tempRouteList.Contains(route)) return;

                if (e == null)
                {
                    var nativeRoute = r.Routes.First();

                    this.SetRouteData(route, nativeRoute);

                    this._routes.Add(nativeRoute.Polyline, new TKOverlayItem<TKRoute, MKPolylineRenderer>(route));
                    this.Map.AddOverlay(nativeRoute.Polyline);

                    route.PropertyChanged += OnRoutePropertyChanged;

                    this.MapFunctions.RaiseRouteCalculationFinished(route);
                }
                else
                {
                    var routeCalculationError = new TKRouteCalculationError(route, e.ToString());
                    this.MapFunctions.RaiseRouteCalculationFailed(routeCalculationError);
                }
            });
        }
        /// <summary>
        /// When a property of a route changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRoutePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var route = (TKRoute)sender;

            if (route == null) return;

            var item = this._routes.SingleOrDefault(i => i.Value.Overlay.Equals(route));
            if (item.Key == null) return;

            if (e.PropertyName != TKRoute.TravelModelProperty &&
                e.PropertyName != TKRoute.SourceProperty &&
                e.PropertyName != TKRoute.DestinationProperty)
            {
                if (e.PropertyName == TKPolyline.ColorPropertyName)
                {
                    item.Value.Renderer.FillColor = item.Value.Overlay.Color.ToUIColor();
                    item.Value.Renderer.StrokeColor = item.Value.Overlay.Color.ToUIColor();
                }
                else if (e.PropertyName == TKPolyline.LineWidthProperty)
                {
                    item.Value.Renderer.LineWidth = item.Value.Overlay.LineWidth;
                }
                return;
            }

            item.Value.Overlay.PropertyChanged -= OnRoutePropertyChanged;

            this.Map.RemoveOverlay(item.Key);
            this._routes.Remove(item.Key);

            this.AddRoute(route);
        }
        /// <summary>
        /// Adds a circle to the map
        /// </summary>
        /// <param name="circle">The circle to add</param>
        private void AddCircle(TKCircle circle)
        {
            var mkCircle = MKCircle.Circle(circle.Center.ToLocationCoordinate(), circle.Radius);
            
            this._circles.Add(mkCircle, new TKOverlayItem<TKCircle,MKCircleRenderer>(circle));
            this.Map.AddOverlay(mkCircle);

            circle.PropertyChanged += OnCirclePropertyChanged;
        }
        /// <summary>
        /// When a property of a circle changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnCirclePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var circle = (TKCircle)sender;

            if (circle == null) return;

            var item = this._circles.SingleOrDefault(i => i.Value.Overlay.Equals(circle));
            if (item.Key == null) return;

            if (e.PropertyName != TKCircle.CenterPropertyName &&
                e.PropertyName != TKCircle.RadiusPropertyName)
            {
                if (e.PropertyName == TKCircle.ColorPropertyName)
                {
                    item.Value.Renderer.FillColor = item.Value.Overlay.Color.ToUIColor();
                }
                else if (e.PropertyName == TKCircle.StrokeColorPropertyName)
                {
                    item.Value.Renderer.StrokeColor = item.Value.Overlay.StrokeColor.ToUIColor();
                }
                else if (e.PropertyName == TKCircle.StrokeWidthPropertyName)
                {
                    item.Value.Renderer.LineWidth = item.Value.Overlay.StrokeWidth;
                }
                return;
            }

            this.Map.RemoveOverlay(item.Key);
            this._circles.Remove(item.Key);

            var mkCircle = MKCircle.Circle(circle.Center.ToLocationCoordinate(), circle.Radius);
            this._circles.Add(mkCircle, new TKOverlayItem<TKCircle,MKCircleRenderer>(circle));
            this.Map.AddOverlay(mkCircle);
        }
        /// <summary>
        /// When a property of the route changes, re-add the <see cref="MKPolyline"/>
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnLinePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var line = (TKPolyline)sender;

            if(line == null) return;

            var item = this._lines.SingleOrDefault(i => i.Value.Overlay.Equals(line));
            if (item.Key == null) return;

            if (e.PropertyName != TKPolyline.LineCoordinatesPropertyName)
            {
                if (e.PropertyName == TKPolyline.ColorPropertyName)
                {
                    item.Value.Renderer.FillColor = item.Value.Overlay.Color.ToUIColor();
                    item.Value.Renderer.StrokeColor = item.Value.Overlay.Color.ToUIColor();
                }
                else if (e.PropertyName == TKPolyline.LineWidthProperty)
                {
                    item.Value.Renderer.LineWidth = item.Value.Overlay.LineWidth;
                }
                return;
            }

            this.Map.RemoveOverlay(item.Key);
            this._lines.Remove(item.Key);

            var polyLine = MKPolyline.FromCoordinates(line.LineCoordinates.Select(i => i.ToLocationCoordinate()).ToArray());
            this._lines.Add(polyLine, new TKOverlayItem<TKPolyline,MKPolylineRenderer>(line));
            this.Map.AddOverlay(polyLine);
        }
        /// <summary>
        /// When a property of the pin changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnPinPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TKCustomMapPin.TitlePropertyName ||
                e.PropertyName == TKCustomMapPin.SubititlePropertyName ||
                (e.PropertyName == TKCustomMapPin.PositionPropertyName && this._isDragging))
                return;

            var formsPin = (TKCustomMapPin)sender;
            var annotation = this.Map.Annotations
                .OfType<TKCustomMapAnnotation>()
                .SingleOrDefault(i => i.CustomPin.Equals(formsPin));

            if (annotation == null) return;

            var annotationView = this.Map.ViewForAnnotation(annotation);
            if (annotationView == null) return;

            switch (e.PropertyName)
            {
                case TKCustomMapPin.ImagePropertyName:
                    this.UpdateImage(annotationView, formsPin);
                    break;
                case TKCustomMapPin.DefaultPinColorPropertyName:
                    this.UpdateImage(annotationView, formsPin);
                    break;
                case TKCustomMapPin.IsDraggablePropertyName:
                    annotationView.Draggable = formsPin.IsDraggable;
                    break;
                case TKCustomMapPin.IsVisiblePropertyName:
                    this.SetAnnotationViewVisibility(annotationView, formsPin);
                    break;
                case TKCustomMapPin.PositionPropertyName:
                    annotation.SetCoordinate(formsPin.Position.ToLocationCoordinate());
                    break;
                case TKCustomMapPin.ShowCalloutPropertyName:
                    annotationView.CanShowCallout = formsPin.ShowCallout;
                    break;
                case TKCustomMapPin.AnchorPropertyName:
                    if(formsPin.Image != null)
                    {
                        annotationView.Layer.AnchorPoint = new CGPoint(formsPin.Anchor.X, formsPin.Anchor.Y);
                    }
                    break;
                case TKCustomMapPin.RotationPropertyName:
                    annotationView.Transform = CGAffineTransform.MakeRotation((float)formsPin.Rotation);
                    break;
                case TKCustomMapPin.IsCalloutClickablePropertyName:
                    this.UpdateAccessoryView(formsPin, annotationView);
                    break;
            }
        }
        /// <summary>
        /// Sets the route data
        /// </summary>
        /// <param name="route">PCL route</param>
        /// <param name="nativeRoute">Native route</param>
        private void SetRouteData(TKRoute route, MKRoute nativeRoute)
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
            
            var region = MKCoordinateRegion.FromMapRect(this.Map.MapRectThatFits(nativeRoute.Polyline.BoundingMapRect, new UIEdgeInsets(15, 15, 15, 15)));

            routeFunctions.SetBounds(new MapSpan(region.Center.ToPosition(), region.Span.LatitudeDelta, region.Span.LongitudeDelta));
            routeFunctions.SetIsCalculated(true);
        }
        /// <summary>
        /// Set the visibility of an annotation view
        /// </summary>
        /// <param name="annotationView">The annotation view</param>
        /// <param name="pin">The forms pin</param>
        private void SetAnnotationViewVisibility(MKAnnotationView annotationView, TKCustomMapPin pin)
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
        private async void UpdateImage(MKAnnotationView annotationView, TKCustomMapPin pin)
        {
            if (pin.Image != null)
            {
                // If this is the case, we need to get a whole new annotation view. 
                if (annotationView.GetType() == typeof (MKPinAnnotationView))
                {
                    this.Map.RemoveAnnotation(annotationView.Annotation);
                    this.Map.AddAnnotation(new TKCustomMapAnnotation(pin));
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
                    pinAnnotationView.AnimatesDrop = true;

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
            }
        }
        /// <summary>
        /// Updates the tiles and adds or removes the overlay
        /// </summary>
        private void UpdateTileOptions()
        {
            if (this.Map == null) return;

            if(this._tileOverlay != null)
            {
                this.Map.RemoveOverlay(this._tileOverlay);
                this._tileOverlay = null;
            }

            if(this.FormsMap != null && this.FormsMap.TilesUrlOptions != null)
            {
                this._tileOverlay = new MKTileOverlay(
                    this.FormsMap.TilesUrlOptions.TilesUrl
                        .Replace("{0}", "{x}")
                        .Replace("{1}", "{y}")
                        .Replace("{2}", "{z}"));
                
                this._tileOverlay.TileSize = new CGSize(
                    this.FormsMap.TilesUrlOptions.TileWidth,
                    this.FormsMap.TilesUrlOptions.TileHeight);

                this._tileOverlay.MinimumZ = this.FormsMap.TilesUrlOptions.MinimumZoomLevel;
                this._tileOverlay.MaximumZ = this.FormsMap.TilesUrlOptions.MaximumZoomLevel;

                this._tileOverlay.CanReplaceMapContent = true;
                this.Map.AddOverlay(this._tileOverlay);
            }
        }
        /// <summary>
        /// Sets the selected pin
        /// </summary>
        private void SetSelectedPin()
        {
            var customAnnotion = this._selectedAnnotation as TKCustomMapAnnotation;

            if (customAnnotion != null)
            {
                if (customAnnotion.CustomPin.Equals(this.FormsMap.SelectedPin)) return;

                var annotationView = this.Map.ViewForAnnotation(customAnnotion);
                if(annotationView != null)
                    annotationView.Selected = false;

                this._selectedAnnotation = null;
            }
            if (this.FormsMap.SelectedPin != null)
            {
                var selectedAnnotation = this.Map.Annotations
                    .OfType<TKCustomMapAnnotation>()
                    .SingleOrDefault(i => i.CustomPin.Equals(this.FormsMap.SelectedPin));

                if (selectedAnnotation != null)
                {
                    var annotationView = this.Map.ViewForAnnotation(selectedAnnotation);
                    this._selectedAnnotation = selectedAnnotation;
                    if (annotationView != null)
                    {
                        this.Map.SelectAnnotation(selectedAnnotation, true);
                    }
                    this.MapFunctions.RaisePinSelected(null);
                }
            }
        }
        /// <summary>
        /// Sets the center of the map
        /// </summary>
        private void SetMapCenter()
        {
            if(this.FormsMap == null || this.Map == null) return;

            if (!this.FormsMap.MapCenter.Equals(this.Map.CenterCoordinate.ToPosition()))
            {
                this.Map.SetCenterCoordinate(this.FormsMap.MapCenter.ToLocationCoordinate(), this.FormsMap.IsRegionChangeAnimated);   
            }
        }
        /// <summary>
        /// Sets traffic enabled on the map
        /// </summary>
        private void UpdateShowTraffic()
        {
            if (this.FormsMap == null || this.Map == null) return;

            var showsTrafficAvailable = this.Map.RespondsToSelector(new Selector("showsTraffic"));
            if (!showsTrafficAvailable)
            {
                return;
            }

            this.Map.ShowsTraffic = this.FormsMap.ShowTraffic;
        }
        /// <summary>
        /// Updates the map region when changed
        /// </summary>
        private void UpdateMapRegion()
        {
            if (this.FormsMap == null || this.FormsMap.MapRegion == null) return;

            if(this.FormsMap.MapRegion != this.FormsMap.VisibleRegion)
            {
                this.MoveToMapRegion(this.FormsMap.MapRegion, this.FormsMap.IsRegionChangeAnimated);
            }
        }
        /// <summary>
        /// Calculates the closest distance of a point to a polyline
        /// </summary>
        /// <param name="pt">The point</param>
        /// <param name="poly">The polyline</param>
        /// <returns>The closes distance</returns>
        private double DistanceOfPoint(MKMapPoint pt, MKPolyline poly)
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
        private double MetersFromPixel(int px, CGPoint pt)
        {
            CGPoint ptB = new CGPoint(pt.X + px, pt.Y);

            CLLocationCoordinate2D coordA = this.Map.ConvertPoint(pt, this.Map);
            CLLocationCoordinate2D coordB = this.Map.ConvertPoint(ptB, this.Map);
            
            return MKGeometry.MetersBetweenMapPoints(MKMapPoint.FromCoordinate(coordA), MKMapPoint.FromCoordinate(coordB));
        }
        /// <summary>
        /// Convert a <see cref="MKCoordinateRegion"/> to <see cref="MKMapRect"/>
        /// http://stackoverflow.com/questions/9270268/convert-mkcoordinateregion-to-mkmaprect
        /// </summary>
        /// <param name="region">Region to convert</param>
        /// <returns>The map rect</returns>
        private MKMapRect RegionToRect(MKCoordinateRegion region)
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
        private void UnregisterCollections(TKCustomMap map)
        {
            this.UnregisterCollection(map.CustomPins, this.OnCollectionChanged, this.OnPinPropertyChanged);
            this.UnregisterCollection(map.Routes, this.OnRouteCollectionChanged, this.OnRoutePropertyChanged);
            this.UnregisterCollection(map.Polylines, this.OnLineCollectionChanged, this.OnLinePropertyChanged);
            this.UnregisterCollection(map.Circles, this.OnCirclesCollectionChanged, this.OnCirclePropertyChanged);
            this.UnregisterCollection(map.Polygons, this.OnPolygonsCollectionChanged, this.OnPolygonPropertyChanged);
        }
        /// <summary>
        /// Unregisters one collection and all of its items
        /// </summary>
        /// <param name="collection">The collection to unregister</param>
        /// <param name="observableHandler">The <see cref="NotifyCollectionChangedEventHandler"/> of the collection</param>
        /// <param name="propertyHandler">The <see cref="PropertyChangedEventHandler"/> of the collection items</param>
        private void UnregisterCollection(
            IEnumerable collection, 
            NotifyCollectionChangedEventHandler observableHandler, 
            PropertyChangedEventHandler propertyHandler)
        {
            if (collection == null) return;

            var observable = collection as INotifyCollectionChanged;
            if(observable != null)
            {
                observable.CollectionChanged -= observableHandler;
            }
            foreach(INotifyPropertyChanged obj in collection)
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
                    UIGraphics.BeginImageContextWithOptions(this.Frame.Size, false, 0.0f);
                    this.Layer.RenderInContext(UIGraphics.GetCurrentContext());

                    img = UIGraphics.GetImageFromCurrentImageContext();
                    UIGraphics.EndImageContext();
                });
            });
            return img.AsPNG().ToArray();
        }
        /// <inheritdoc/>
        public void FitMapRegionToPositions(IEnumerable<Position> positions, bool animate = false)
        {
            if(this.Map == null) return;

            MKMapRect zoomRect = MKMapRect.Null;

            foreach(var position in positions)
            {
                MKMapPoint point = MKMapPoint.FromCoordinate(position.ToLocationCoordinate());
                MKMapRect pointRect = new MKMapRect(point.X, point.Y, 0.1, 0.1);
                zoomRect = MKMapRect.Union(zoomRect, pointRect);
            }
            this.Map.SetVisibleMapRect(zoomRect, animate);
        }
        /// <inheritdoc/>
        public void MoveToMapRegion(MapSpan region, bool animate)
        {
            if (this.Map == null) return;

            var coordinateRegion = MKCoordinateRegion.FromDistance(
                region.Center.ToLocationCoordinate(), 
                region.Radius.Meters * 2, 
                region.Radius.Meters * 2);
            
            this.Map.SetRegion(coordinateRegion, animate);
        }
        /// <inheritdoc/>
        public void FitToMapRegions(IEnumerable<MapSpan> regions, bool animate)
        {
            if (this.Map == null) return;

            MKMapRect rect = MKMapRect.Null;
            foreach(var region in regions)
            {
                rect = MKMapRect.Union(
                    rect,
                    this.RegionToRect(
                        MKCoordinateRegion.FromDistance(
                            region.Center.ToLocationCoordinate(),
                            region.Radius.Meters * 2,
                            region.Radius.Meters * 2)));
            }
            this.Map.SetVisibleMapRect(rect, new UIEdgeInsets(15, 15, 15, 15), animate);
        }
        /// <inheritdoc />
        public IEnumerable<Position> ScreenLocationsToGeocoordinates(params Point[] screenLocations)
        {
            if (this.Map == null)
                throw new InvalidOperationException("Map not initialized");

            return screenLocations.Select(i => this.Map.ConvertPoint(i.ToCGPoint(), this.Map).ToPosition());
        }
        /// <summary>
        /// Returns the <see cref="TKCustomMapPin"/> by the native <see cref="IMKAnnotation"/>
        /// </summary>
        /// <param name="annotation">The annotation to search with</param>
        /// <returns>The forms pin</returns>
        protected TKCustomMapPin GetPinByAnnotation(IMKAnnotation annotation)
        {
            var customAnnotation = annotation as TKCustomMapAnnotation;
            if (customAnnotation == null) return null;

            return customAnnotation.CustomPin;
        }
    }
}
