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

namespace TK.CustomMap.Utilities
{
    /// <summary>
    /// This is a java to c# port from 
    /// https://github.com/googlemaps/android-maps-utils/blob/dba3b0d8a9657ebab8c67a4f50bd731437a229bc/library/src/com/google/maps/android/MathUtil.java
    /// </summary>
    public class GmsMathUtils
    {
        public const double EarthRadius = 6371009;

        /// <summary>
        ///  Restrict x to the range [low, high].
        /// </summary>
        /// <param name="x"></param>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <returns></returns>
        public static double Clamp(double x, double low, double high)
        {
            return x < low ? low : (x > high ? high : x);
        }
        /// <summary>
        /// Wraps the given value into the inclusive-exclusive interval between min and max.
        /// </summary>
        /// <param name="n">The value to wrap.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns></returns>
        public static double Wrap(double n, double min, double max)
        {
            return (n >= min && n < max) ? n : (Mod(n - min, max - min) + min);
        }
        /// <summary>
        /// Returns the non-negative remainder of x / m.
        /// </summary>
        /// <param name="x">The operand.</param>
        /// <param name="m">The modulus.</param>
        /// <returns></returns>
        public static double Mod(double x, double m)
        {
            return ((x % m) + m) % m;
        }
        /// <summary>
        /// Returns mercator Y corresponding to latitude.
        /// See http://en.wikipedia.org/wiki/Mercator_projection .
        /// </summary>
        /// <param name="lat"></param>
        /// <returns></returns>
        public static double Mercator(double lat)
        {
            return Math.Log(Math.Tan(lat * 0.5 + Math.PI / 4));
        }
        /// <summary>
        /// Returns latitude from mercator Y.
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public static double InverseMercator(double y)
        {
            return 2 * Math.Atan(Math.Exp(y)) - Math.PI / 2;
        }
        /// <summary>
        /// Returns haversine(angle-in-radians).
        /// hav(x) == (1 - cos(x)) / 2 == sin(x / 2)^2.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double Hav(double x)
        {
            double sinHalf = Math.Sin(x * 0.5);
            return sinHalf * sinHalf;
        }
        /// <summary>
        /// Computes inverse haversine. Has good numerical stability around 0.
        /// arcHav(x) == acos(1 - 2 * x) == 2 * asin(sqrt(x)).
        /// The argument must be in [0, 1], and the result is positive.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double ArcHav(double x)
        {
            return 2 * Math.Asin(Math.Sqrt(x));
        }
        /// <summary>
        /// Given h==hav(x), returns sin(abs(x)).
        /// </summary>
        /// <param name="h"></param>
        /// <returns></returns>
        public static double SinFromHav(double h)
        {
            return 2 * Math.Sqrt(h * (1 - h));
        }
        /// <summary>
        /// Returns hav(asin(x))
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double HavFromSin(double x)
        {
            double x2 = x * x;
            return x2 / (1 + Math.Sqrt(1 - x2)) * .5;
        }
        /// <summary>
        /// Returns sin(arcHav(x) + arcHav(y)).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static double SinSumFromHav(double x, double y)
        {
            double a = Math.Sqrt(x * (1 - x));
            double b = Math.Sqrt(y * (1 - y));
            return 2 * (a + b - 2 * (a * y + b * x));
        }
        /// <summary>
        /// Returns hav() of distance from (lat1, lng1) to (lat2, lng2) on the unit sphere.
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lat2"></param>
        /// <param name="dLng"></param>
        /// <returns></returns>
        public static double HavDistance(double lat1, double lat2, double dLng)
        {
            return Hav(lat1 - lat2) + Hav(dLng) * Math.Cos(lat1) * Math.Cos(lat2);
        }
    }
}
