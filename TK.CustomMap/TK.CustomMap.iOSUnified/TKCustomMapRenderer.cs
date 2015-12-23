using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using CoreGraphics;
using Foundation;
using MapKit;
using TK.CustomMap;
using TK.CustomMap.iOSUnified;
using TK.CustomMap.Overlays;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.iOS;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(TKCustomMap), typeof(TKCustomMapRenderer))]

namespace TK.CustomMap.iOSUnified
{
    /// <summary>
    /// iOS Renderer of <see cref="TK.CustomMap.TKCustomMap"/>
    /// </summary>
    public class TKCustomMapRenderer : MapRenderer
    {

        private const string AnnotationIdentifier = "TKCustomAnnotation";
        private const string AnnotationIdentifierDefaultPin = "TKCustomAnnotationDefaultPin";

        private readonly Dictionary<MKPolyline, TKOverlayItem<TKRoute, MKPolylineRenderer>> _routes = new Dictionary<MKPolyline, TKOverlayItem<TKRoute, MKPolylineRenderer>>();
        private readonly Dictionary<MKPolyline, TKOverlayItem<TKPolyline, MKPolylineRenderer>> _lines = new Dictionary<MKPolyline, TKOverlayItem<TKPolyline, MKPolylineRenderer>>();
        private readonly Dictionary<MKCircle, TKOverlayItem<TKCircle, MKCircleRenderer>> _circles = new Dictionary<MKCircle, TKOverlayItem<TKCircle, MKCircleRenderer>>();
        private readonly Dictionary<MKPolygon, TKOverlayItem<TKPolygon, MKPolygonRenderer>> _polygons = new Dictionary<MKPolygon, TKOverlayItem<TKPolygon, MKPolygonRenderer>>();

        private bool _firstUpdate = true;
        private bool _isDragging;
        private IMKAnnotation _selectedAnnotation;

        private MKMapView Map
        {
            get { return this.Control as MKMapView; }
        }
        private TKCustomMap FormsMap
        {
            get { return this.Element as TKCustomMap; }
        }
  
        /// <summary>
        /// Dummy function to avoid linker.
        /// </summary>
        [Preserve]
        public static void InitMapRenderer()
        { }
        /// <inheritdoc/>
        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);
            
            if (e.OldElement != null || this.FormsMap == null || this.Map == null) return;
            
            this.Map.GetViewForAnnotation = this.GetViewForAnnotation;
            this.Map.OverlayRenderer = this.GetOverlayRenderer; 
            this.Map.DidSelectAnnotationView += OnDidSelectAnnotationView;
            this.Map.RegionChanged += OnMapRegionChanged;
            this.Map.ChangedDragState += OnChangedDragState;
            this.Map.CalloutAccessoryControlTapped += OnMapCalloutAccessoryControlTapped;

            this.Map.AddGestureRecognizer(new UILongPressGestureRecognizer(this.OnMapLongPress));

            var customGestureRecognizer = new UITapGestureRecognizer(this.OnMapClicked)
            {
                ShouldReceiveTouch = (recognizer, touch) => !(touch.View is MKAnnotationView)
            };

            this.Map.AddGestureRecognizer(customGestureRecognizer);

            if (this.FormsMap.CustomPins != null)
            {
                this.UpdatePins();
                this.FormsMap.CustomPins.CollectionChanged += OnCollectionChanged;
            }
            this.SetMapCenter();
            this.UpdateRoutes();
            this.UpdateLines();
            this.UpdateCircles();
            this.UpdatePolygons();
            this.FormsMap.PropertyChanged += OnMapPropertyChanged;
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
                var route = this._routes[polyline];
                if (route == null)
                {
                    // not a route, check if it is a line
                    var line = this._lines[polyline];

                    if (line == null) return null;

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

                if (route.Renderer == null)
                {
                    route.Renderer = new MKPolylineRenderer(polyline);
                }
                route.Renderer.FillColor = route.Overlay.Color.ToUIColor();
                route.Renderer.LineWidth = route.Overlay.LineWidth;
                route.Renderer.StrokeColor = route.Overlay.Color.ToUIColor();
                return route.Renderer;
            }

            var mkCircle = overlay as MKCircle;
            if (mkCircle != null)
            {
                var circle = this._circles[mkCircle];
                if (circle == null) return null;

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
                if (polygon == null) return null;

                if (polygon.Renderer == null)
                {
                    polygon.Renderer = new MKPolygonRenderer(mkPolygon);
                }
                
                polygon.Renderer.FillColor = polygon.Overlay.Color.ToUIColor();
                polygon.Renderer.StrokeColor = polygon.Overlay.StrokeColor.ToUIColor();
                polygon.Renderer.LineWidth = polygon.Overlay.StrokeWidth;
                return polygon.Renderer;
            }
            return null;
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
                this._firstUpdate = true;
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
                this.UpdatePins();
            }
            else if(e.PropertyName == TKCustomMap.PolygonsProperty.PropertyName)
            {
                this.UpdatePolygons();
            }
            else if (e.PropertyName == TKCustomMap.RoutesProperty.PropertyName)
            {
                this.UpdateRoutes();
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
                    this.Map.AddAnnotation(new TKCustomMapAnnotation(pin));
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (TKCustomMapPin pin in e.OldItems)
                {
                    if (!this.FormsMap.CustomPins.Contains(pin))
                    {
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
                foreach (TKCustomMapAnnotation annotation in this.Map.Annotations.OfType<TKCustomMapAnnotation>())
                {
                    annotation.CustomPin.PropertyChanged -= OnPinPropertyChanged;
                }
                this._firstUpdate = true;
                this.UpdatePins();
            }
        }
        /// <summary>
        /// When the accessory control of a callout gets tapped
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnMapCalloutAccessoryControlTapped(object sender, MKMapViewAccessoryTappedEventArgs e)
        {
            if (this.FormsMap.CalloutClickedCommand.CanExecute(null))
            {
                this.FormsMap.CalloutClickedCommand.Execute(null);
            }
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
                if (this.FormsMap.PinDragEndCommand != null && this.FormsMap.PinDragEndCommand.CanExecute(annotation.CustomPin))
                {
                    this.FormsMap.PinDragEndCommand.Execute(annotation.CustomPin);
                }
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
        private void OnDidSelectAnnotationView(object sender, MKAnnotationViewEventArgs e)
        {
            var pin = e.View.Annotation as TKCustomMapAnnotation;
            if(pin == null) return;

            this._selectedAnnotation = e.View.Annotation;
            this.FormsMap.SelectedPin = pin.CustomPin;
            
            if (this.FormsMap.PinSelectedCommand != null && this.FormsMap.PinSelectedCommand.CanExecute(pin.CustomPin))
            {
                this.FormsMap.PinSelectedCommand.Execute(pin.CustomPin);
            }
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

            if (this.FormsMap.Routes != null && this.FormsMap.RouteClickedCommand != null)
            {
                foreach (var route in this.FormsMap.Routes.Where(i => i.Selectable))
                {
                    if (this.RouteHit(pixelLocation, this._routes.Single(i => i.Value.Overlay.Equals(route)).Key))
                    {
                        if (this.FormsMap.RouteClickedCommand.CanExecute(route))
                        {
                            this.FormsMap.RouteClickedCommand.Execute(route);
                            return;
                        }
                    }
                }
            }

            if (this.FormsMap.MapClickedCommand != null && this.FormsMap.MapClickedCommand.CanExecute(coordinate.ToPosition()))
            {
                this.FormsMap.MapClickedCommand.Execute(coordinate.ToPosition());
            }

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

            if (this.FormsMap.MapLongPressCommand != null && this.FormsMap.MapLongPressCommand.CanExecute(coordinate.ToPosition()))
            {
                this.FormsMap.MapLongPressCommand.Execute(coordinate.ToPosition());
            }
        }
        /// <summary>
        /// Get the view for the annotation
        /// </summary>
        /// <param name="mapView">The map</param>
        /// <param name="annotation">The annotation</param>
        /// <returns>The annotation view</returns>
        private MKAnnotationView GetViewForAnnotation(MKMapView mapView, IMKAnnotation annotation)
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
                if(customAnnotation.CustomPin.Image != null)
                    annotationView = new MKAnnotationView();
                else
                    annotationView = new MKPinAnnotationView(customAnnotation, AnnotationIdentifier);
            }
            else 
            {
                annotationView.Annotation = customAnnotation;
            }
            annotationView.CanShowCallout = customAnnotation.CustomPin.ShowCallout;
            annotationView.Draggable = customAnnotation.CustomPin.IsDraggable;
            annotationView.Selected = this._selectedAnnotation != null && customAnnotation.Equals(this._selectedAnnotation);
            this.SetAnnotationViewVisibility(annotationView, customAnnotation.CustomPin);
            this.UpdateImage(annotationView, customAnnotation.CustomPin);

            if (FormsMap.CalloutClickedCommand != null)
            {
                var button = new UIButton(UIButtonType.InfoLight)
                {
                    Frame = new CGRect(0, 0, 23, 23),
                    HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                    VerticalAlignment = UIControlContentVerticalAlignment.Center
                };
                annotationView.RightCalloutAccessoryView = button;
            }
            
            return annotationView;
        }
        /// <summary>
        /// Creates the annotations
        /// </summary>
        private void UpdatePins()
        {
            this.Map.RemoveAnnotations(this.Map.Annotations);

            if (this.FormsMap.CustomPins == null || !this.FormsMap.CustomPins.Any()) return;

            foreach (var i in FormsMap.CustomPins)
            {
                if (this._firstUpdate)
                {
                    i.PropertyChanged += OnPinPropertyChanged;
                }
                var pin = new TKCustomMapAnnotation(i);
                this.Map.AddAnnotation(pin);
            }
            this._firstUpdate = false;

            if (this.FormsMap.PinsReadyCommand != null && this.FormsMap.PinsReadyCommand.CanExecute(this.FormsMap))
            {
                this.FormsMap.PinsReadyCommand.Execute(this.FormsMap);
            }
        }
        /// <summary>
        /// Creates the lines
        /// </summary>
        private void UpdateLines(bool firstUpdate = true)
        {
            if (this._lines.Any())
            {
                this.Map.RemoveOverlays(this._lines.Select(i => i.Key).ToArray());
                this._lines.Clear();
            }

            if (this.FormsMap.Polylines == null) return;

            foreach (var line in this.FormsMap.Polylines)
            {
                this.AddLine(line);
            }

            if (firstUpdate)
            {
                var observAble = this.FormsMap.Polylines as ObservableCollection<TKPolyline>;
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
            if (this._routes.Any())
            {
                this.Map.RemoveOverlays(this._routes.Select(i => i.Key).ToArray());
                this._routes.Clear();
            }
            if (this.FormsMap.Routes == null) return;

            foreach (var route in this.FormsMap.Routes)
            {
                this.AddRoute(route);
            }

            if (firstUpdate)
            {
                var observAble = this.FormsMap.Routes as ObservableCollection<TKRoute>;
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
                foreach (var route in this._routes)
                {
                    route.Value.Overlay.PropertyChanged -= OnPolygonPropertyChanged;
                }
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
                var observAble = this.FormsMap.Circles as ObservableCollection<TKCircle>;
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
                var observAble = this.FormsMap.Polygons as ObservableCollection<TKPolygon>;
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
                else if (e.PropertyName == TKOverlay.ColorPropertyName)
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
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var circle in this._circles)
                {
                    circle.Value.Overlay.PropertyChanged -= OnCirclePropertyChanged;
                }
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
                foreach (var route in this._lines)
                {
                    route.Value.Overlay.PropertyChanged -= OnLinePropertyChanged;
                }
                this.UpdateLines(false);
            }
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
            var req = new MKDirectionsRequest
            {
                Source = new MKMapItem(new MKPlacemark(route.Source.ToLocationCoordinate(), new MKPlacemarkAddress())),
                Destination =
                    new MKMapItem(new MKPlacemark(route.Destination.ToLocationCoordinate(), new MKPlacemarkAddress())),
                TransportType = route.TravelMode.ToTransportType()
            };

            var directions = new MKDirections(req);
            directions.CalculateDirections((r, e) => 
            {
                if (r != null && r.Routes != null && r.Routes.Any())
                {
                    var nativeRoute = r.Routes.First();

                    this.SetRouteData(route, nativeRoute);

                    this._routes.Add(nativeRoute.Polyline, new TKOverlayItem<TKRoute, MKPolylineRenderer>(route));
                    this.Map.AddOverlay(nativeRoute.Polyline);

                    route.PropertyChanged += OnRoutePropertyChanged;

                    if (this.FormsMap.RouteCalculationFinishedCommand != null && this.FormsMap.RouteCalculationFinishedCommand.CanExecute(route))
                    {
                        this.FormsMap.RouteCalculationFinishedCommand.Execute(route);
                    }
                }
                else
                {
                    if (this.FormsMap.RouteCalculationFailedCommand != null && this.FormsMap.RouteCalculationFailedCommand.CanExecute(route))
                    {
                        this.FormsMap.RouteCalculationFailedCommand.Execute(route);
                    }
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
                if (e.PropertyName == TKOverlay.ColorPropertyName)
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
                if (e.PropertyName == TKOverlay.ColorPropertyName)
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
                if (e.PropertyName == TKOverlay.ColorPropertyName)
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

            routeFunctions.SetBounds(
                MapSpan.FromCenterAndRadius(
                    nativeRoute.Polyline.Coordinate.ToPosition(),
                    Distance.FromKilometers(
                        route.Source.DistanceTo(route.Destination))));
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

                var image = await new ImageLoaderSourceHandler().LoadImageAsync(pin.Image);
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
        /// Sets the selected pin
        /// </summary>
        private void SetSelectedPin()
        {
            var customAnnotion = this._selectedAnnotation as TKCustomMapAnnotation;

            if (customAnnotion != null)
            {
                if (customAnnotion.CustomPin.Equals(this.FormsMap.SelectedPin)) return;

                var annotationView = this.Map.ViewForAnnotation(customAnnotion);
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
                    if (annotationView != null)
                    {
                        annotationView.Selected = true;
                    }
                    this._selectedAnnotation = selectedAnnotation;

                    if (this.FormsMap.PinSelectedCommand != null && this.FormsMap.PinSelectedCommand.CanExecute(null))
                    {
                        this.FormsMap.PinSelectedCommand.Execute(null);
                    }
                }
            }
        }
        /// <summary>
        /// Sets the center of the map
        /// </summary>
        private void SetMapCenter()
        {
            if (!this.FormsMap.MapCenter.Equals(this.Map.CenterCoordinate.ToPosition()))
            {
                this.Map.SetCenterCoordinate(this.FormsMap.MapCenter.ToLocationCoordinate(), this.FormsMap.AnimateMapCenterChange);   
            }
        }
        /// <summary>
        /// Returns the distance of a point to a polyline in meters 
        /// from http://paulbourke.net/geometry/pointlineplane/DistancePoint.java
        /// </summary>
        /// <param name="pt">From Point</param>
        /// <param name="poly">To Poly</param>
        /// <returns>Distance in meters</returns>
        private double DistanceOfPoint(MKMapPoint pt, MKPolyline poly)
        {
            double distance = float.MaxValue;
            for (var n = 0; n < poly.PointCount - 1; n++) {

                var ptA = poly.Points[n];
                var ptB = poly.Points[n + 1];

                var xDelta = ptB.X - ptA.X;
                var yDelta = ptB.Y - ptA.Y;

                if (xDelta == 0.0 && yDelta == 0.0) {

                    // Points must not be equal
                    continue;
                }

                var u = ((pt.X - ptA.X) * xDelta + (pt.Y - ptA.Y) * yDelta) / (xDelta * xDelta + yDelta * yDelta);
                MKMapPoint ptClosest;
                if (u < 0.0) {

                    ptClosest = ptA;
                }
                else if (u > 1.0) {

                    ptClosest = ptB;
                }
                else {

                    ptClosest = new MKMapPoint(ptA.X + u * xDelta, ptA.Y + u * yDelta);
                }
                
                distance = Math.Min(distance, MKMapPoint.ToCoordinate(ptClosest).ToPosition().DistanceTo(MKMapPoint.ToCoordinate(pt).ToPosition()) * 1000);
            }

            return distance;
        }
        /// <summary>
        /// Converts pixel to meters
        /// </summary>
        /// <param name="px">Pixel</param>
        /// <param name="point">Point</param>
        /// <returns>Pixel in meters</returns>
        private double MetersFromPixel(int px, CGPoint point)
        {
            var ptB = new CGPoint(point.X + px, point.Y);

            var coordA = this.Map.ConvertPoint(point, this.Map);
            var coordB = this.Map.ConvertPoint(ptB, this.Map);

            return coordA.ToPosition().DistanceTo(coordB.ToPosition())*1000;
        }

        /// <summary>
        /// Check whether a route was hit
        /// </summary>
        /// <param name="tapPoint">Point of tap</param>
        /// <param name="polyline">The route polyline</param>
        /// <returns><value>true</value> if hit</returns>
        private bool RouteHit(CGPoint tapPoint, MKPolyline polyline)
        {
            var coordinate = this.Map.ConvertPoint(tapPoint, this.Map);
            var maxMeters = this.MetersFromPixel(22, tapPoint);

            var distance = this.DistanceOfPoint(MKMapPoint.FromCoordinate(coordinate), polyline);

            if (distance <= maxMeters)
            {
                return true;
            }
            return false;
        }
    }
}
