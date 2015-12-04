using System;
using System.Linq;
using Android.Gms.Maps;
using Xamarin.Forms;
using Xamarin.Forms.Maps.Android;
using TK.CustomMap;
using Xamarin.Forms.Platform.Android;
using TK.CustomMap.Droid;
using Android.Gms.Maps.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(TKCustomMap), typeof(TKCustomMapRenderer))]
namespace TK.CustomMap.Droid
{
      /// <summary>
      /// Android Renderer of <see cref="TK.CustomMap.TKCustomMap"/>
      /// </summary>
    public class TKCustomMapRenderer : MapRenderer, IOnMapReadyCallback
    {

        private readonly Dictionary<TKCustomMapPin, Marker> _markers = new Dictionary<TKCustomMapPin, Marker>();
        private Marker _selectedMarker;
        private bool _isDragging;
        private bool _firstUpdate = true;

        private GoogleMap _googleMap;

        private TKCustomMap FormsMap
        {
            get { return this.Element as TKCustomMap; }
        }

        /// <inheritdoc />
        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);

            MapView mapView = this.Control as MapView;
            if (mapView == null) return;

            if (this.FormsMap != null && this._googleMap == null)
            {
                mapView.GetMapAsync(this);
                this.FormsMap.PropertyChanged += FormsMapPropertyChanged;

                if (e.OldElement == null)
                {
                    if (this.FormsMap.CustomPins != null)
                    {
                        this.FormsMap.CustomPins.CollectionChanged += OnCustomPinsCollectionChanged;
                    }
                }
            }
        }
        /// <summary>
        /// When a property of the Forms map changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void FormsMapPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(this._googleMap == null) return;

            if (e.PropertyName == TKCustomMap.CustomPinsProperty.PropertyName)
            {
                this._firstUpdate = true;
                this.UpdatePins();
            }
            else if (e.PropertyName == TKCustomMap.SelectedPinProperty.PropertyName)
            {
                this.SetSelectedItem();
            }
            else if (e.PropertyName == TKCustomMap.MapCenterProperty.PropertyName)
            {
                this.MoveToCenter();
            }
        }
        /// <summary>
        /// When the map is ready to use
        /// </summary>
        /// <param name="googleMap">The map instance</param>
        public void OnMapReady(GoogleMap googleMap)
        {
            this._googleMap = googleMap;

            this._googleMap.MarkerClick += OnMarkerClick;
            this._googleMap.MapClick += OnMapClick;
            this._googleMap.MapLongClick += OnMapLongClick;
            this._googleMap.MarkerDragEnd += OnMarkerDragEnd;
            this._googleMap.MarkerDrag += OnMarkerDrag;
            this._googleMap.CameraChange += OnCameraChange;
            this._googleMap.MarkerDragStart += OnMarkerDragStart;

            this.MoveToCenter();
            this.UpdatePins();
        }
        /// <summary>
        /// Dragging process
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnMarkerDrag(object sender, GoogleMap.MarkerDragEventArgs e)
        {
            var item = this._markers.SingleOrDefault(i => i.Value.Id.Equals(e.Marker.Id));
            if (item.Key == null) return;

            item.Key.Position = e.Marker.Position.ToPosition();
        }
        /// <summary>
        /// When a dragging starts
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnMarkerDragStart(object sender, GoogleMap.MarkerDragStartEventArgs e)
        {
            this._isDragging = true;
        }
        /// <summary>
        /// When the camera position changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnCameraChange(object sender, GoogleMap.CameraChangeEventArgs e)
        {
            this.FormsMap.MapCenter = e.Position.Target.ToPosition();
        }
        /// <summary>
        /// When a pin gets clicked
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnMarkerClick(object sender, GoogleMap.MarkerClickEventArgs e)
        {
            if (this.FormsMap == null) return;
            var item = this._markers.SingleOrDefault(i => i.Value.Id.Equals(e.Marker.Id));
            if (item.Key == null) return;

            this._selectedMarker = e.Marker;
            this.FormsMap.SelectedPin = item.Key;
            if (item.Key.ShowCallout)
            {
                item.Value.ShowInfoWindow();
            }
        }
        /// <summary>
        /// When a drag of a marker ends
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnMarkerDragEnd(object sender, GoogleMap.MarkerDragEndEventArgs e)
        {
            this._isDragging = false;

            if (this.FormsMap == null) return;

            var pin = this._markers.SingleOrDefault(i => i.Value.Id.Equals(e.Marker.Id));
            if (pin.Key == null) return;

            if (this.FormsMap.PinDragEndCommand != null && this.FormsMap.PinDragEndCommand.CanExecute(pin.Key))
            {
                this.FormsMap.PinDragEndCommand.Execute(pin.Key);
            }
        }
        /// <summary>
        /// When a long click was performed on the map
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnMapLongClick(object sender, GoogleMap.MapLongClickEventArgs e)
        {
            if (this.FormsMap == null || this.FormsMap.MapLongPressCommand == null) return;

            var position = e.Point.ToPosition();

            if (this.FormsMap.MapLongPressCommand.CanExecute(position))
            {
                this.FormsMap.MapLongPressCommand.Execute(position);
            }
        }
        /// <summary>
        /// When the map got tapped
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private void OnMapClick(object sender, GoogleMap.MapClickEventArgs e)
        {
            if (this.FormsMap == null || this.FormsMap.MapClickedCommand == null) return;

            var position = e.Point.ToPosition();

            if (this.FormsMap.MapClickedCommand.CanExecute(position))
            {
                this.FormsMap.MapClickedCommand.Execute(position);
            }
        }
        /// <summary>
        /// Updates the markers when a pin gets added or removed in the collection
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private async void OnCustomPinsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (TKCustomMapPin pin in e.NewItems)
                {
                    await this.AddPin(pin);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (TKCustomMapPin pin in e.OldItems)
                {
                    if (!this.FormsMap.CustomPins.Contains(pin))
                    {
                        this.RemovePin(pin);
                    }
                }
            }
        }
        /// <summary>
        /// When a property of a pin changed
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Arguments</param>
        private async void OnPinPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var pin = sender as TKCustomMapPin;
            if (pin == null) return;

            var marker = this._markers[pin];
            if (marker == null) return;

            switch (e.PropertyName)
            {
                case TKCustomMapPin.TitlePropertyName:
                    marker.Title = pin.Title;
                    break;
                case TKCustomMapPin.SubititlePropertyName:
                    marker.Snippet = pin.Subtitle;
                    break;
                case TKCustomMapPin.ImagePropertyName:
                    if (pin.Image != null)
                    {
                        var icon = await new ImageLoaderSourceHandler().LoadImageAsync(pin.Image, this.Context);
                        marker.SetIcon(BitmapDescriptorFactory.FromBitmap(icon));
                    }
                    else
                    {
                        marker.SetIcon(BitmapDescriptorFactory.DefaultMarker());
                    }
                    break;
                case TKCustomMapPin.PositionPropertyName:
                    if (!this._isDragging)
                    {
                        marker.Position = new LatLng(pin.Position.Latitude, pin.Position.Longitude);
                    }
                    break;
                case TKCustomMapPin.IsVisiblePropertyName:
                    marker.Visible = pin.IsVisible;
                    break;
            }
        }
        /// <summary>
        /// Creates all Markers on the map
        /// </summary>
        private async void UpdatePins()
        {
            if (this._googleMap == null) return;

            this._googleMap.Clear();
            this._markers.Clear();

            var items = this.FormsMap.CustomPins;

            if (items == null) return;

            var imageSourceHandler = new ImageLoaderSourceHandler();
            foreach (var pin in items)
            {
                await this.AddPin(pin);

                if (this._firstUpdate)
                {
                    pin.PropertyChanged += OnPinPropertyChanged;
                }
            }
            this._firstUpdate = false;

            if (this.FormsMap.PinsReadyCommand != null && this.FormsMap.PinsReadyCommand.CanExecute(this.FormsMap))
            {
                this.FormsMap.PinsReadyCommand.Execute(this.FormsMap);
            }
        }
        /// <summary>
        /// Adds a marker to the map
        /// </summary>
        /// <param name="pin">The Forms Pin</param>
        private async Task AddPin(TKCustomMapPin pin)
        {
            var markerWithIcon = new MarkerOptions();
            markerWithIcon.SetPosition(new LatLng(pin.Position.Latitude, pin.Position.Longitude));

            if (!string.IsNullOrWhiteSpace(pin.Title))
                markerWithIcon.SetTitle(pin.Title);
            if (!string.IsNullOrWhiteSpace(pin.Subtitle))
                markerWithIcon.SetSnippet(pin.Subtitle);

            BitmapDescriptor bitmap = null;
            try
            {
                if (pin.Image != null)
                {
                    var icon = await new ImageLoaderSourceHandler().LoadImageAsync(pin.Image, this.Context);
                    bitmap = BitmapDescriptorFactory.FromBitmap(icon);
                }
                else
                {
                    bitmap = BitmapDescriptorFactory.DefaultMarker();
                }
            }
            catch (Exception)
            {
                bitmap = BitmapDescriptorFactory.DefaultMarker();
            }
            markerWithIcon.SetIcon(bitmap);
            markerWithIcon.Draggable(pin.IsDraggable);
            markerWithIcon.Visible(pin.IsVisible);

            this._markers.Add(pin, this._googleMap.AddMarker(markerWithIcon));
        }
        /// <summary>
        /// Remove a pin from the map and the internal dictionary
        /// </summary>
        /// <param name="pin">The pin to remove</param>
        private void RemovePin(TKCustomMapPin pin)
        {
            var item = this._markers[pin];
            if(item == null) return;

            if (item.Id.Equals(this._selectedMarker.Id))
            {
                this.FormsMap.SelectedPin = null;
            }

            item.Remove();
            pin.PropertyChanged -= OnPinPropertyChanged;
            this._markers.Remove(pin);
        }
        /// <summary>
        /// Set the selected item on the map
        /// </summary>
        private void SetSelectedItem()
        {
            if (this._selectedMarker != null)
            {
                this._selectedMarker.HideInfoWindow();
                this._selectedMarker = null;
            }
            if (this.FormsMap.SelectedPin != null)
            {
                var selectedPin = this._markers[this.FormsMap.SelectedPin];
                this._selectedMarker = selectedPin;
                if (this.FormsMap.SelectedPin.ShowCallout)
                {
                    selectedPin.ShowInfoWindow();
                }
            }
        }
        /// <summary>
        /// Move the google map to the map center
        /// </summary>
        private void MoveToCenter()
        {
            if (!this.FormsMap.MapCenter.Equals(this._googleMap.CameraPosition.Target.ToPosition()))
            {
                var cameraUpdate = CameraUpdateFactory.NewLatLng(this.FormsMap.MapCenter.ToLatLng());

                if (this.FormsMap.AnimateMapCenterChange)
                {
                    this._googleMap.AnimateCamera(cameraUpdate);
                }
                else
                {
                    this._googleMap.MoveCamera(cameraUpdate);
                }
            }
        }
    }
}