using CoreLocation;
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
    }
}
