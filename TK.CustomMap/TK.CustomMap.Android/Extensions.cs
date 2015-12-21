using Android.Gms.Maps.Model;
using TK.CustomMap.Api.Google;
using TK.CustomMap.Overlays;
using Xamarin.Forms.Maps;

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
    }
}