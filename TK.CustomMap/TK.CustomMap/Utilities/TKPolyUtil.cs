/*
 * Copyright 2008, 2013 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Maps;

namespace TK.CustomMap
{
    /// <summary>
    /// This is a java to c# port from
    /// https://github.com/googlemaps/android-maps-utils/blob/dba3b0d8a9657ebab8c67a4f50bd731437a229bc/library/src/com/google/maps/android/PolyUtil.java
    /// </summary>
    public class TKPolyUtil
    {
         private const double DEFAULT_TOLERANCE = 0.1;  // meters.

        /// <summary>
        /// Returns tan(latitude-at-lng3) on the great circle (lat1, lng1) to (lat2, lng2). lng1==0.
        /// See http://williams.best.vwh.net/avform.htm .
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lat2"></param>
        /// <param name="lng2"></param>
        /// <param name="lng3"></param>
        /// <returns></returns>
        private static double TanLatGC(double lat1, double lat2, double lng2, double lng3)
        {
            return (Math.Tan(lat1) * Math.Sin(lng2 - lng3) + Math.Tan(lat2) * Math.Sin(lng3)) / Math.Sin(lng2);
        }
        /// <summary>
        /// Returns mercator(latitude-at-lng3) on the Rhumb line (lat1, lng1) to (lat2, lng2). lng1==0.
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lat2"></param>
        /// <param name="lng2"></param>
        /// <param name="lng3"></param>
        /// <returns></returns>
        private static double MercatorLatRhumb(double lat1, double lat2, double lng2, double lng3)
        {
            return (TKMathUtils.Mercator(lat1) * (lng2 - lng3) + TKMathUtils.Mercator(lat2) * lng3) / lng2;
        }
        /// <summary>
        /// Computes whether the vertical segment (lat3, lng3) to South Pole intersects the segment
        /// (lat1, lng1) to (lat2, lng2).
        /// Longitudes are offset by -lng1; the implicit lng1 becomes 0.
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lat2"></param>
        /// <param name="lng2"></param>
        /// <param name="lat3"></param>
        /// <param name="lng3"></param>
        /// <param name="geodesic"></param>
        /// <returns></returns>
        private static bool Intersects(double lat1, double lat2, double lng2,
                                      double lat3, double lng3, bool geodesic)
        {
            // Both ends on the same side of lng3.
            if ((lng3 >= 0 && lng3 >= lng2) || (lng3 < 0 && lng3 < lng2))
            {
                return false;
            }
            // Point is South Pole.
            if (lat3 <= -Math.PI / 2)
            {
                return false;
            }
            // Any segment end is a pole.
            if (lat1 <= -Math.PI / 2 || lat2 <= -Math.PI / 2 || lat1 >= Math.PI / 2 || lat2 >= Math.PI / 2)
            {
                return false;
            }
            if (lng2 <= -Math.PI)
            {
                return false;
            }
            double linearLat = (lat1 * (lng2 - lng3) + lat2 * lng3) / lng2;
            // Northern hemisphere and point under lat-lng line.
            if (lat1 >= 0 && lat2 >= 0 && lat3 < linearLat)
            {
                return false;
            }
            // Southern hemisphere and point above lat-lng line.
            if (lat1 <= 0 && lat2 <= 0 && lat3 >= linearLat)
            {
                return true;
            }
            // North Pole.
            if (lat3 >= Math.PI / 2)
            {
                return true;
            }
            // Compare lat3 with latitude on the GC/Rhumb segment corresponding to lng3.
            // Compare through a strictly-increasing function (Math.Tan() or mercator()) as convenient.
            return geodesic ?
                Math.Tan(lat3) >= TanLatGC(lat1, lat2, lng2, lng3) :
                TKMathUtils.Mercator(lat3) >= MercatorLatRhumb(lat1, lat2, lng2, lng3);
        }

        /// <summary>
        /// Computes whether the given point lies inside the specified polygon.
        /// The polygon is always cosidered closed, regardless of whether the last point equals
        /// the first or not.
        /// Inside is defined as not containing the South Pole -- the South Pole is always outside.
        ///  The polygon is formed of great circle segments if geodesic is true, and of rhumb
        /// (loxodromic) segments otherwise.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polygon"></param>
        /// <param name="geodesic"></param>
        /// <returns></returns>
        public static bool ContainsLocation(Position point, IEnumerable<Position> polygon, bool geodesic) 
        {
            int size = polygon.Count();
            if (size == 0)  return false;

            double lat3 = point.Latitude.ToRadian();
            double lng3 = point.Longitude.ToRadian();
            Position prev = polygon.Last();
            double lat1 = prev.Latitude.ToRadian();
            double lng1 = prev.Longitude.ToRadian();
            int nIntersect = 0;

            foreach(var point2 in polygon)
            {
                double dLng3 = TKMathUtils.Wrap(lng3 - lng1, -Math.PI, Math.PI);
                // Special case: point equal to vertex is inside.
                if (lat3 == lat1 && dLng3 == 0) {
                    return true;
                }
                double lat2 = point2.Latitude.ToRadian();
                double lng2 = point2.Longitude.ToRadian();
                // Offset longitudes by -lng1.
                if (Intersects(lat1, lat2, TKMathUtils.Wrap(lng2 - lng1, -Math.PI, Math.PI), lat3, dLng3, geodesic)) {
                    ++nIntersect;
                }
                lat1 = lat2;
                lng1 = lng2;
            }
            return (nIntersect & 1) != 0;
        }
        /// <summary>
        /// Computes whether the given point lies on or near the edge of a polygon, within a specified
        /// tolerance in meters. The polygon edge is composed of great circle segments if geodesic
        /// is true, and of Rhumb segments otherwise. The polygon edge is implicitly closed -- the
        /// closing segment between the first point and the last point is included.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polygon"></param>
        /// <param name="geodesic"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool IsLocationOnEdge(Position point, IEnumerable<Position> polygon, bool geodesic,
                                           double tolerance)
        {
            return IsLocationOnEdgeOrPath(point, polygon, true, geodesic, tolerance);
        }
        /// <summary>
        /// Same as <see cref="IsLocationOnEdge"/>
        /// with a default tolerance of 0.1 meters.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polygon"></param>
        /// <param name="geodesic"></param>
        /// <returns></returns>
        public static bool IsLocationOnEdge(Position point, IEnumerable<Position> polygon, bool geodesic)
        {
            return IsLocationOnEdge(point, polygon, geodesic, DEFAULT_TOLERANCE);
        }
        /// <summary>
        ///  Computes whether the given point lies on or near a polyline, within a specified
        /// tolerance in meters. The polyline is composed of great circle segments if geodesic
        /// is true, and of Rhumb segments otherwise. The polyline is not closed -- the closing
        /// segment between the first point and the last point is not included.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polyline"></param>
        /// <param name="geodesic"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool IsLocationOnPath(Position point, IEnumerable<Position> polyline,
                                           bool geodesic, double tolerance)
        {
            return IsLocationOnEdgeOrPath(point, polyline, false, geodesic, tolerance);
        }
        /// <summary>
        /// Same as <see cref="IsLocationOnPath"/>
        /// with a default tolerance of 0.1 meters.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polyline"></param>
        /// <param name="geodesic"></param>
        /// <returns></returns>
        public static bool IsLocationOnPath(Position point, IEnumerable<Position> polyline,
                                           bool geodesic)
        {
            return IsLocationOnPath(point, polyline, geodesic, DEFAULT_TOLERANCE);
        }

        private static bool IsLocationOnEdgeOrPath(Position point, IEnumerable<Position> poly, bool closed,
                                                  bool geodesic, double toleranceEarth) 
        {
            int size = poly.Count();
            if (size == 0) return false;
            
            double tolerance = toleranceEarth / TKMathUtils.EarthRadius;
            double havTolerance = TKMathUtils.Hav(tolerance);
            double lat3 = point.Latitude.ToRadian();
            double lng3 = point.Longitude.ToRadian();
            Position prev = poly.ElementAt(closed ? size - 1 : 0); 
            double lat1 = prev.Latitude.ToRadian();
            double lng1 = prev.Longitude.ToRadian();
            if (geodesic) 
            {
                foreach(var point2 in poly)
                {
                    double lat2 = point2.Latitude.ToRadian();
                    double lng2 = point2.Longitude.ToRadian();
                    if (IsOnSegmentGC(lat1, lng1, lat2, lng2, lat3, lng3, havTolerance)) {
                        return true;
                    }
                    lat1 = lat2;
                    lng1 = lng2;
                }
            } 
            else 
            {
                // We project the points to mercator space, where the Rhumb segment is a straight line,
                // and compute the geodesic distance between point3 and the closest point on the
                // segment. This method is an approximation, because it uses "closest" in mercator
                // space which is not "closest" on the sphere -- but the error is small because
                // "tolerance" is small.
                double minAcceptable = lat3 - tolerance;
                double maxAcceptable = lat3 + tolerance;
                double y1 = TKMathUtils.Mercator(lat1);
                double y3 = TKMathUtils.Mercator(lat3);
                double[] xTry = new double[3];

                foreach(var point2 in poly)
                {
                    double lat2 = point2.Latitude.ToRadian();
                    double y2 = TKMathUtils.Mercator(lat2);
                    double lng2 = point2.Longitude.ToRadian();
                    if (Math.Max(lat1, lat2) >= minAcceptable && Math.Min(lat1, lat2) <= maxAcceptable) {
                        // We offset longitudes by -lng1; the implicit x1 is 0.
                        double x2 = TKMathUtils.Wrap(lng2 - lng1, -Math.PI, Math.PI);
                        double x3Base = TKMathUtils.Wrap(lng3 - lng1, -Math.PI, Math.PI);
                        xTry[0] = x3Base;
                        // Also explore wrapping of x3Base around the world in both directions.
                        xTry[1] = x3Base + 2 * Math.PI;
                        xTry[2] = x3Base - 2 * Math.PI;

                        foreach(var x3 in xTry)
                        {
                            double dy = y2 - y1;
                            double len2 = x2 * x2 + dy * dy;
                            double t = len2 <= 0 ? 0 : TKMathUtils.Clamp((x3 * x2 + (y3 - y1) * dy) / len2, 0, 1);
                            double xClosest = t * x2;
                            double yClosest = y1 + t * dy;
                            double latClosest = TKMathUtils.InverseMercator(yClosest);
                            double havDist = TKMathUtils.HavDistance(lat3, latClosest, x3 - xClosest);
                            if (havDist < havTolerance) {
                                return true;
                            }
                        }
                    }
                    lat1 = lat2;
                    lng1 = lng2;
                    y1 = y2;
                }
            }
            return false;
        }
        private static bool IsOnSegmentGC(double lat1, double lng1, double lat2, double lng2,
                                         double lat3, double lng3, double havTolerance)
        {
            double havDist13 = TKMathUtils.HavDistance(lat1, lat3, lng1 - lng3);
            if (havDist13 <= havTolerance)
            {
                return true;
            }
            double havDist23 = TKMathUtils.HavDistance(lat2, lat3, lng2 - lng3);
            if (havDist23 <= havTolerance)
            {
                return true;
            }
            double sinBearing = SinDeltaBearing(lat1, lng1, lat2, lng2, lat3, lng3);
            double sinDist13 = TKMathUtils.SinFromHav(havDist13);
            double havCrossTrack = TKMathUtils.HavFromSin(sinDist13 * sinBearing);
            if (havCrossTrack > havTolerance)
            {
                return false;
            }
            double havDist12 = TKMathUtils.HavDistance(lat1, lat2, lng1 - lng2);
            double term = havDist12 + havCrossTrack * (1 - 2 * havDist12);
            if (havDist13 > term || havDist23 > term)
            {
                return false;
            }
            if (havDist12 < 0.74)
            {
                return true;
            }
            double cosCrossTrack = 1 - 2 * havCrossTrack;
            double havAlongTrack13 = (havDist13 - havCrossTrack) / cosCrossTrack;
            double havAlongTrack23 = (havDist23 - havCrossTrack) / cosCrossTrack;
            double sinSumAlongTrack = TKMathUtils.SinSumFromHav(havAlongTrack13, havAlongTrack23);
            return sinSumAlongTrack > 0;  // Compare with half-circle == PI using sign of sin().
        }
        /// <summary>
        /// Returns sin(initial bearing from (lat1,lng1) to (lat3,lng3) minus initial bearing
        /// from (lat1, lng1) to (lat2,lng2)).
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lng1"></param>
        /// <param name="lat2"></param>
        /// <param name="lng2"></param>
        /// <param name="lat3"></param>
        /// <param name="lng3"></param>
        /// <returns></returns>
        private static double SinDeltaBearing(double lat1, double lng1, double lat2, double lng2,
                                          double lat3, double lng3)
        {
            double sinLat1 = Math.Sin(lat1);
            double cosLat2 = Math.Cos(lat2);
            double cosLat3 = Math.Cos(lat3);
            double lat31 = lat3 - lat1;
            double lng31 = lng3 - lng1;
            double lat21 = lat2 - lat1;
            double lng21 = lng2 - lng1;
            double a = Math.Sin(lng31) * cosLat3;
            double c = Math.Sin(lng21) * cosLat2;
            double b = Math.Sin(lat31) + 2 * sinLat1 * cosLat3 * TKMathUtils.Hav(lng31);
            double d = Math.Sin(lat21) + 2 * sinLat1 * cosLat2 * TKMathUtils.Hav(lng21);
            double denom = (a * a + b * b) * (c * c + d * d);
            return denom <= 0 ? 1 : (a * d - b * c) / Math.Sqrt(denom);
        }

    }
}
