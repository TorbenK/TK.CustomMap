using System;
using System.Globalization;
using Xamarin.Forms.Maps;

namespace TK.CustomMap
{
    /// <summary>
    /// Extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Convert a <see cref="Position"/> to a <see cref="string"/>
        /// </summary>
        /// <param name="self">Self struct</param>
        /// <returns><see cref="Position"/> as <see cref="string"/></returns>
        public static string AsString(this Position self)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0},{1}", self.Latitude, self.Longitude);
        }
        /// <summary>
        /// Calculates the distance to the given point in a straight line. 
        /// </summary>
        /// <param name="self">Self instance</param>
        /// <param name="target">The target position</param>
        /// <param name="inMiles">If <value>true</value> the distance is calculated in miles, else in kilometers</param>
        /// <returns>The distance in miles or kilometers</returns>
        public static double DistanceTo(this Position self, Position target, bool inMiles = false)
        {
            return GetDistance(self, target, inMiles);   
        }
        /// <summary>
        /// Convert to Radians.
        /// </summary>
        /// <param name="val">Value in degrees</param>
        /// <returns>Value in radians</returns>
        public static double ToRadian(this double val)
        {
            return (Math.PI / 180) * val;
        }
        /// <summary>
        /// Convert to Degrees.
        /// </summary>
        /// <param name="val">Value in radians</param>
        /// <returns>Value in degrees</returns>
        public static double ToDegrees(this double val)
        {
            return val / (Math.PI / 180);
        }
        /// <summary>
        /// Calculate the distance between two positions https://en.wikipedia.org/wiki/Haversine_formula)
        /// </summary>
        /// <param name="coordinateA">From coordinate</param>
        /// <param name="coordinateB">To coordinate</param>
        /// <param name="inMiles">Calculate in miles if true, otherwise in kilometers</param>
        /// <returns>The distance in kilometers or miles</returns>
        private static double GetDistance(Position coordinateA, Position coordinateB, bool inMiles)
        {
            var earthRadius = inMiles ? 3959 : 6371;

            var dLat = (coordinateB.Latitude - coordinateA.Latitude).ToRadian();
            var dLon = (coordinateB.Longitude - coordinateA.Longitude).ToRadian();

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(coordinateA.Latitude.ToRadian()) * Math.Cos(coordinateB.Latitude.ToRadian()) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
            var d = earthRadius * c;

            return d;
        }
    }
}
