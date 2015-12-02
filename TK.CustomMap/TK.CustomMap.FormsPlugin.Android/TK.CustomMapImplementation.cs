using System;
using Android.Gms.Maps;
using Xamarin.Forms;
using Xamarin.Forms.Maps.Android;
using TK.CustomMap;
using Xamarin.Forms.Platform.Android;
using TK.CustomMap.Droid;
using Android.Gms.Maps.Model;
using System.Collections.Generic;

[assembly: ExportRenderer(typeof(TKCustomMap), typeof(TKCustomMapRenderer))]
namespace TK.CustomMap.Droid
{
      /// <summary>
      /// TK.CustomMap Renderer
      /// </summary>
    public class TKCustomMapRenderer : MapRenderer, IOnMapReadyCallback
    {

        private readonly Dictionary<TKCustomMapPin, Marker> _markers = new Dictionary<TKCustomMapPin, Marker>();
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

                if (e.OldElement == null)
                {
                    if (this.FormsMap.CustomPins != null)
                    {
                        this.FormsMap.CustomPins.CollectionChanged += OnCustomPinsCollectionChanged;
                        this.UpdatePins();
                    }
                }
            }
        }

        private void OnCustomPinsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// When the map is ready to use
        /// </summary>
        /// <param name="googleMap">The map instance</param>
        public void OnMapReady(GoogleMap googleMap)
        {
            this._googleMap = googleMap;

            this._googleMap.MapClick += OnMapClick;
            this._googleMap.MapLongClick += OnMapLongClick;
            this._googleMap.MarkerDragEnd += OnMarkerDragEnd;
        }

        void OnMarkerDragEnd(object sender, GoogleMap.MarkerDragEndEventArgs e)
        {
            
        }

        void OnMapLongClick(object sender, GoogleMap.MapLongClickEventArgs e)
        {
            throw new NotImplementedException();
        }

        void OnMapClick(object sender, GoogleMap.MapClickEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void UpdatePins()
        {
            if (this._googleMap == null) return;

            this._googleMap.Clear();
            this._markers.Clear();

            var items = this.FormsMap.CustomPins;

            if (items == null) return;

            var imageSourceHandler = new ImageLoaderSourceHandler();
            foreach (var item in items)
            {
                var markerWithIcon = new MarkerOptions();
                markerWithIcon.SetPosition(new LatLng(item.Position.Latitude, item.Position.Longitude));

                if (!string.IsNullOrWhiteSpace(item.Title))
                    markerWithIcon.SetTitle(item.Title);
                if(!string.IsNullOrWhiteSpace(item.Subtitle))
                    markerWithIcon.SetSnippet(item.Subtitle);

                BitmapDescriptor bitmap = null;
                try
                {
                    if (item.Image != null)
                    {
                        var icon = await imageSourceHandler.LoadImageAsync(item.Image, this.Context);
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
                markerWithIcon.Draggable(item.IsDraggable);
                markerWithIcon.Visible(item.IsVisible);

                if (this._firstUpdate)
                {
                    item.PropertyChanged += OnItemPropertyChanged;
                }
                this._markers.Add(item, this._googleMap.AddMarker(markerWithIcon));
            }
            this._firstUpdate = false;
        }

        private async void OnItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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
                case TKCustomMapPin.AddressPropertyName:
                    marker.Snippet = pin.Subtitle;
                    break;
                case TKCustomMapPin.IconPropertyName:
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
                    marker.Position = new LatLng(pin.Position.Latitude, pin.Position.Longitude);
                    break;
                case TKCustomMapPin.IsVisiblePropertyName:
                    marker.Visible = pin.IsVisible;
                    break;
            }
        }
    }
}