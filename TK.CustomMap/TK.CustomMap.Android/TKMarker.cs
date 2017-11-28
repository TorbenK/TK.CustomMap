using Android.Content;
using Android.Gms.Maps.Model;
using Com.Google.Maps.Android.Clustering;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms.Platform.Android;

namespace TK.CustomMap.Droid
{
    internal class TKMarker : Java.Lang.Object, IClusterItem
    {
        Context _context;

        public TKMarker(TKCustomMapPin pin, Context context)
        {
            Pin = pin;
            _context = context;
        }

        public TKCustomMapPin Pin { get;  set; }

        public LatLng Position => Pin.Position.ToLatLng();

        public string Snippet => Pin.Subtitle;

        public string Title => Pin.Title;

        public Marker Marker { get; internal set; }

        public async Task HandlePropertyChangedAsync(PropertyChangedEventArgs e, bool isDragging)
        {
            switch (e.PropertyName)
            {
                case nameof(TKCustomMapPin.Title):
                    Marker.Title = Pin.Title;
                    break;
                case nameof(TKCustomMapPin.Subtitle):
                    Marker.Snippet = Pin.Subtitle;
                    break;
                case nameof(TKCustomMapPin.Image):
                    await UpdateImageAsync();
                    break;
                case nameof(TKCustomMapPin.DefaultPinColor):
                    await UpdateImageAsync();
                    break;
                case nameof(TKCustomMapPin.Position):
                    if (!isDragging)
                    {
                        Marker.Position = new LatLng(Pin.Position.Latitude, Pin.Position.Longitude);
                    }
                    break;
                case nameof(TKCustomMapPin.IsVisible):
                    Marker.Visible = Pin.IsVisible;
                    break;
                case nameof(TKCustomMapPin.Anchor):
                    if (Pin.Image != null)
                    {
                        Marker.SetAnchor((float)Pin.Anchor.X, (float)Pin.Anchor.Y);
                    }
                    break;
                case nameof(TKCustomMapPin.IsDraggable):
                    Marker.Draggable = Pin.IsDraggable;
                    break;
                case nameof(TKCustomMapPin.Rotation):
                    Marker.Rotation = (float)Pin.Rotation;
                    break;
            }
        }

        public async Task InitializeMarkerOptionsAsync(MarkerOptions markerOptions, bool setPosition = true)
        {
            if (setPosition)
            {
                markerOptions.SetPosition(new LatLng(Pin.Position.Latitude, Pin.Position.Longitude));
            }

            if (!string.IsNullOrWhiteSpace(Pin.Title))
                markerOptions.SetTitle(Pin.Title);
            if (!string.IsNullOrWhiteSpace(Pin.Subtitle))
                markerOptions.SetSnippet(Pin.Subtitle);

            await UpdateImageAsync(markerOptions);
            markerOptions.Draggable(Pin.IsDraggable);
            markerOptions.Visible(Pin.IsVisible);
            markerOptions.SetRotation((float)Pin.Rotation);
            if (Pin.Image != null)
            {
                markerOptions.Anchor((float)Pin.Anchor.X, (float)Pin.Anchor.Y);
            }
            markerOptions.Flat(true);

        }
        /// <summary>
        /// Updates the image of a pin
        /// </summary>
        /// <param name="pin">The forms pin</param>
        /// <param name="markerOptions">The native marker options</param>
        async Task UpdateImageAsync()
        {
            BitmapDescriptor bitmap;
            try
            {
                if (Pin.Image != null)
                {
                    bitmap = BitmapDescriptorFactory.FromBitmap(await Pin.Image.ToBitmap(_context));
                }
                else
                {
                    if (Pin.DefaultPinColor != Xamarin.Forms.Color.Default)
                    {
                        var hue = Pin.DefaultPinColor.ToAndroid().GetHue();
                        bitmap = BitmapDescriptorFactory.DefaultMarker(System.Math.Min(hue, 359.99f));
                    }
                    else
                    {
                        bitmap = BitmapDescriptorFactory.DefaultMarker();
                    }
                }
            }
            catch (System.Exception)
            {
                bitmap = BitmapDescriptorFactory.DefaultMarker();
            }
            Marker.SetIcon(bitmap);
        }
        /// <summary>
        /// Updates the image of a pin
        /// </summary>
        /// <param name="pin">The forms pin</param>
        /// <param name="markerOptions">The native marker options</param>
        async Task UpdateImageAsync(MarkerOptions markerOptions)
        {
            BitmapDescriptor bitmap;
            try
            {
                if (Pin.Image != null)
                {
                    bitmap = BitmapDescriptorFactory.FromBitmap(await Pin.Image.ToBitmap(_context));
                }
                else
                {
                    if (Pin.DefaultPinColor != Xamarin.Forms.Color.Default)
                    {
                        var hue = Pin.DefaultPinColor.ToAndroid().GetHue();
                        bitmap = BitmapDescriptorFactory.DefaultMarker(System.Math.Min(hue, 359.99f));
                    }
                    else
                    {
                        bitmap = BitmapDescriptorFactory.DefaultMarker();
                    }
                }
            }
            catch (System.Exception)
            {
                bitmap = BitmapDescriptorFactory.DefaultMarker();
            }
            markerOptions.SetIcon(bitmap);
        }
    }
}