using System;
using System.Collections.Generic;
using TK.CustomMap;
using System.Linq;
using Xamarin.Forms;
using TK.CustomMap.iOSUnified;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms.Maps.iOS;
using MapKit;
using System.ComponentModel;
using UIKit;
using System.Collections.Specialized;
using Foundation;
using TK.CustomMap.Overlays;

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

        private readonly Dictionary<MKPolyline, TKRoute> _routes = new Dictionary<MKPolyline, TKRoute>();
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

            this.Map.AddGestureRecognizer(new UILongPressGestureRecognizer(this.OnMapLongPress));
            this.Map.AddGestureRecognizer(new UITapGestureRecognizer(this.OnMapClicked));

            if (this.FormsMap.CustomPins != null)
            {
                this.UpdatePins();
                this.FormsMap.CustomPins.CollectionChanged += OnCollectionChanged;
            }
            this.SetMapCenter();
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
                var route = this._routes[polyline];
                return new MKPolylineRenderer(polyline) 
                {
                    FillColor = route.LineColor.ToUIColor(),
                    LineWidth = route.LineWidth,
                    StrokeColor = route.LineColor.ToUIColor(),
                };
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
        }
        /// <summary>
        /// When the collection of pins changed
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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
            
            return annotationView;
        }
        /// <summary>
        /// Creates the annotations
        /// </summary>
        private void UpdatePins()
        {
            this.Map.RemoveAnnotations(this.Map.Annotations);

            if (this.FormsMap.CustomPins == null) return;

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
        private void UpdateRoutes()
        {
            this.Map.RemoveOverlays(this._routes.Select(i => i.Key).ToArray());

            if (this.FormsMap.Routes == null) return;

            foreach (var route in this.FormsMap.Routes)
            {
                var polyLine = MKPolyline.FromCoordinates(route.RouteCoordinates.Select(i => i.ToLocationCoordinate()).ToArray());
                this._routes.Add(polyLine, route);
                this.Map.AddOverlay(polyLine);

                route.PropertyChanged += OnRoutePropertyChanged;
            }
        }
        /// <summary>
        /// When a property of the route changes, re-add the <see cref="MKPolyline"/>
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnRoutePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var route = (TKRoute)sender;

            if(route == null) return;

            var item = this._routes.SingleOrDefault(i => i.Value.Equals(route));
            if (item.Key == null) return;

            this.Map.RemoveOverlay(item.Key);
            this._routes.Remove(item.Key);

            var polyLine = MKPolyline.FromCoordinates(route.RouteCoordinates.Select(i => i.ToLocationCoordinate()).ToArray());
            this._routes.Add(polyLine, route);
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
                    pinAnnotationView.PinTintColor = UIColor.Red;
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
    }
}
