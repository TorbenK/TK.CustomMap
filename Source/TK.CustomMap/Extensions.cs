using System;
using System.Collections.Generic;
using System.Linq;
using TK.CustomMap.Overlays;

namespace TK.CustomMap
{
    /// <summary>
    /// Extension methods
    /// </summary>
    public static class Extensions
    {
        // ReSharper disable InconsistentNaming
        public const int wgs84RADIUS = 6378137;
        public const int earthRadiusInMiles = 3959;
        public const int earthRadiusInKilometers = 6371;
        // ReSharper enable InconsistentNaming

        /// <summary>
        /// Convert a <see cref="Position"/> to a <see cref="string"/>
        /// </summary>
        /// <param name="self">Self struct</param>
        /// <returns><see cref="Position"/> as <see cref="string"/></returns>
        [Obsolete("Use position.ToString() instead")]
        public static string AsString(this Position self) => self.ToString();

        /// <summary>
        /// Calculates the distance to the given point in a straight line. 
        /// </summary>
        /// <param name="self">Self instance</param>
        /// <param name="target">The target position</param>
        /// <param name="inMiles">If <value>true</value> the distance is calculated in miles, else in kilometers</param>
        /// <returns>The distance in miles or kilometers</returns>
        public static double DistanceTo(this Position self, Position target, bool inMiles = false) => 
            GetDistance(self, target, inMiles);

        /// <summary>
        /// Convert to Radians.
        /// </summary>
        /// <param name="val">Value in degrees</param>
        /// <returns>Value in radians</returns>
        public static double ToRadian(this double val) => (Math.PI / 180) * val;

        /// <summary>
        /// Convert to Degrees.
        /// </summary>
        /// <param name="val">Value in radians</param>
        /// <returns>Value in degrees</returns>
        public static double ToDegrees(this double val) => val / (Math.PI / 180);

        /// <summary>
        /// Calculate the distance between two positions https://en.wikipedia.org/wiki/Haversine_formula)
        /// </summary>
        /// <param name="coordinateA">From coordinate</param>
        /// <param name="coordinateB">To coordinate</param>
        /// <param name="inMiles">Calculate in miles if true, otherwise in kilometers</param>
        /// <returns>The distance in kilometers or miles</returns>
        public static double GetDistance(this Position coordinateA, Position coordinateB, bool inMiles)
        {
            var earthRadius = inMiles ? earthRadiusInMiles : earthRadiusInKilometers;

            var dLat = (coordinateB.Latitude - coordinateA.Latitude).ToRadian();
            var dLon = (coordinateB.Longitude - coordinateA.Longitude).ToRadian();

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(coordinateA.Latitude.ToRadian()) * Math.Cos(coordinateB.Latitude.ToRadian()) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
            var d = earthRadius * c;

            return d;
        }

        /// <summary>
        /// Gets the area of a <see cref="TKPolygon"/> in  m^2
        /// </summary>
        /// <param name="polygon">The polygon to calculate</param>
        /// <returns>returns the area of a polygon in m^2</returns>
        public static double GetAreaInSquareMeters(this TKPolygon polygon) => 
            GetArea(polygon);

        /// <summary>
        /// Gets the area of a <see cref="TKPolygon"/> (defaults  to  m^2)
        /// </summary>
        /// <param name="polygon">The polygon to calculate</param>
        /// <param name="radius">The radius used in the calculation. Defaults to WGS 84. https://en.wikipedia.org/wiki/World_Geodetic_System</param>
        /// <returns>returns the area of a polygon</returns>
        public static double GetArea(this TKPolygon polygon, double radius = wgs84RADIUS)
        {
            var coordinates = polygon.Coordinates.EnsureCorrectWinding();
            var size = coordinates.Count;
            if (size < 3) return 0;
            var total = 0d;
            var prev = coordinates[size - 1];
            var prevTanLat = Math.Tan((Math.PI / 2 - ToRadian(prev.Latitude)) / 2);
            var prevLng = ToRadian(prev.Longitude);
            // For each edge, accumulate the signed area of the triangle formed by the North Pole
            // and that edge ("polar triangle").
            foreach (var point in coordinates)
            {
                var tanLat = Math.Tan((Math.PI / 2 - ToRadian(point.Latitude)) / 2);
                var lng = ToRadian(point.Longitude);
                total += PolarTriangleArea(tanLat, lng, prevTanLat, prevLng);
                prevTanLat = tanLat;
                prevLng = lng;
            }

            var result = total * (radius * radius);
            return Math.Abs(result);

            double PolarTriangleArea(double tan1, double lng1, double tan2, double lng2)
            {
                var deltaLng = lng1 - lng2;
                var t = tan1 * tan2;
                return 2 * Math.Atan2(t * Math.Sin(deltaLng), 1 + t * Math.Cos(deltaLng));
            }
        }
        
        /// <summary>
        /// Ensures the winding of the collection based on whether it's a polygon or a hole within a polygon
        /// </summary>
        /// <param name="positions">Polygon collection</param>
        /// <param name="isHole"><see cref="positions"/> is a hole</param>
        /// <returns>Returns an list of positions in the correct winding</returns>
        public static IList<Position> EnsureCorrectWinding(this IEnumerable<Position> positions, bool isHole = false)
        {
            var vertices = positions.ToList();

            var clockWiseCount = 0;
            var counterClockWiseCount = 0;
            var p1 = vertices[0];

            for (var i = 1; i < vertices.Count; i++)
            {
                var p2 = vertices[i];
                var p3 = vertices[(i + 1) % vertices.Count];

                var e1 = new Position(p1.Longitude - p2.Longitude, p1.Latitude - p2.Latitude);
                var e2 = new Position(p3.Longitude - p2.Longitude, p3.Latitude - p2.Latitude);

                if (e1.Longitude * e2.Latitude - e1.Latitude * e2.Longitude >= 0)
                    clockWiseCount++;
                else
                    counterClockWiseCount++;

                p1 = p2;
            }

            var isClockwise = clockWiseCount > counterClockWiseCount;

            return isClockwise
                ? isHole
                    ? vertices
                    : vertices.Reverse<Position>().ToList()
                : isHole
                    ? vertices.Reverse<Position>().ToList()
                    : vertices;
        }
        
        /// <summary>
        /// Determines if a <see cref="Position"/> is inside a <see cref="TKPolygon"/> 
        /// </summary>
        /// <param name="polygon">The Polygon to check</param>
        /// <param name="position">The Position to calculate</param>
        /// <returns><c>true</c> if the Position is inside the Polygon, otherwise <c>false</c></returns>
        public static bool Contains(this TKPolygon polygon, Position position)
        {
            var coordinates = polygon.Coordinates;
            
            var result = false;
            var j = coordinates.Count() - 1;
            for (var i = 0; i < coordinates.Count(); i++)
            {
                if ((coordinates[i].Longitude < position.Longitude 
                     && coordinates[j].Longitude >= position.Longitude 
                     || coordinates[j].Longitude < position.Longitude 
                     && coordinates[i].Longitude >= position.Longitude) 
                    && coordinates[i].Latitude + (position.Longitude - coordinates[i].Longitude) / (coordinates[j].Longitude - coordinates[i].Longitude) * (coordinates[j].Latitude - coordinates[i].Latitude) < position.Latitude)
                {
                    result = !result;
                }
                j = i;
            }
            return result;
        }
    }
}
