using System.Threading.Tasks;
using Android.Content;
using Android.Gms.Maps.Model;
using Android.Graphics;
using TK.CustomMap.Overlays;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Platform.Android;
using TK.CustomMap.Api.Google;

namespace TK.CustomMap.Droid
{
    /// <summary>
    /// Extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Convert <see cref="LatLng" /> to <see cref="Position"/>
        /// </summary>
        /// <param name="self">Self instance</param>
        /// <returns>Forms Position</returns>
        public static Position ToPosition(this LatLng self)
        {
            return new Position(self.Latitude, self.Longitude);
        }
        /// <summary>
        /// Convert <see cref="Position" /> to <see cref="LatLng"/>
        /// </summary>
        /// <param name="self">Self instance</param>
        /// <returns>Android Position</returns>
        public static LatLng ToLatLng(this Position self)
        {
            return new LatLng(self.Latitude, self.Longitude);
        }
        /// <summary>
        /// Convert <see cref="TKRouteTravelMode"/> to <see cref="GmsDirectionTravelMode"/>
        /// </summary>
        /// <param name="self">Self instance</param>
        /// <returns>Gms Direction API travel mode</returns>
        public static GmsDirectionTravelMode ToGmsTravelMode(this TKRouteTravelMode self)
        {
            switch (self)
            {
                case TKRouteTravelMode.Driving:
                    return GmsDirectionTravelMode.Driving;
                case TKRouteTravelMode.Walking:
                    return GmsDirectionTravelMode.Walking;
                case TKRouteTravelMode.Any:
                    return GmsDirectionTravelMode.Driving;
                default:
                    return GmsDirectionTravelMode.Driving;
            }
        }
        /// <summary>
        /// Convert a <see cref="ImageSource"/> to the native Android <see cref="Bitmap"/>
        /// </summary>
        /// <param name="source">Self instance</param>
        /// <param name="context">Android Context</param>
        /// <returns>The Bitmap</returns>
        public static async Task<Bitmap> ToBitmap(this ImageSource source, Context context)
        {
            if (source is FileImageSource)
            {
                return await new FileImageSourceHandler().LoadImageAsync(source, context);
            }
            if (source is UriImageSource)
            {
                return await new ImageLoaderSourceHandler().LoadImageAsync(source, context);
            }
            if (source is StreamImageSource)
            {
                return await new StreamImagesourceHandler().LoadImageAsync(source, context);
            }
            return null;
        }
        /// <summary>
        /// Convert a <see cref="Xamarin.Forms.Point"/> to <see cref="Android.Graphics.Point"/>
        /// </summary>
        /// <param name="point">Self</param>
        /// <returns>A Android point</returns>
        public static Android.Graphics.Point ToAndroidPoint(this Xamarin.Forms.Point point)
        {
            return new Android.Graphics.Point((int)point.X, (int)point.Y);
        }
    }
}