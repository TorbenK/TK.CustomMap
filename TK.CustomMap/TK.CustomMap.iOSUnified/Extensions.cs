using CoreLocation;
using MapKit;
using TK.CustomMap.Overlays;
using Xamarin.Forms.Maps;

namespace TK.CustomMap.iOSUnified
{
    /// <summary>
    /// Extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Convert <see cref="Position" /> to <see cref="CLLocationCoordinate2D"/>
        /// </summary>
        /// <param name="self">Self instance</param>
        /// <returns>iOS coordinate</returns>
        public static CLLocationCoordinate2D ToLocationCoordinate(this Position self)
        {
            return new CLLocationCoordinate2D(self.Latitude, self.Longitude);
        }
        /// <summary>
        /// Convert <see cref="CLLocationCoordinate2D" /> to <see cref="Position"/>
        /// </summary>
        /// <param name="self">Self instance</param>
        /// <returns>Forms position</returns>
        public static Position ToPosition(this CLLocationCoordinate2D self)
        {
            return new Position(self.Latitude, self.Longitude);
        }
        /// <summary>
        /// Convert <see cref="MKDirectionsTransportType"/> to <see cref="TKRouteTravelMode"/>
        /// </summary>
        /// <param name="self">Self instance</param>
        /// <returns>The map kit transport type</returns>
        public static MKDirectionsTransportType ToTransportType(this TKRouteTravelMode self)
        {
            switch (self)
            {
                case TKRouteTravelMode.Driving:
                    return MKDirectionsTransportType.Automobile;
                case TKRouteTravelMode.Walking:
                    return MKDirectionsTransportType.Walking;
                case TKRouteTravelMode.Any:
                    return MKDirectionsTransportType.Any;
                default:
                    return MKDirectionsTransportType.Automobile;
            }
        }
    }
}
