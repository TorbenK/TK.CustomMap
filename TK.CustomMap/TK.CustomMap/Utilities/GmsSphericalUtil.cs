/*
 * Copyright 2013 Google Inc.
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

namespace TK.CustomMap.Utilities
{
    /// <summary>
    /// This is a java to c# port from 
    /// https://github.com/googlemaps/android-maps-utils/blob/dba3b0d8a9657ebab8c67a4f50bd731437a229bc/library/src/com/google/maps/android/SphericalUtil.java
    /// </summary>
    public static class GmsSphericalUtil
    {   
        /// <summary>
        ///  Returns the LatLng resulting from moving a distance from an origin
        ///  in the specified heading (expressed in degrees clockwise from north).
        /// </summary>
        /// <param name="from">The LatLng from which to start.</param>
        /// <param name="distance">The distance to travel</param>
        /// <param name="heading">The heading in degrees clockwise from north.</param>
        /// <returns>Position with offset</returns>
        public static Position ComputeOffset(Position from, double distance, double heading)
        {
            distance /= GmsMathUtils.EarthRadius;
            heading = heading.ToRadian();
            // http://williams.best.vwh.net/avform.htm#LL
            double fromLat = from.Latitude.ToRadian();
            double fromLng = from.Longitude.ToRadian();
            double cosDistance = Math.Cos(distance);
            double sinDistance = Math.Sin(distance);
            double sinFromLat = Math.Sin(fromLat);
            double cosFromLat = Math.Cos(fromLat);
            double sinLat = cosDistance * sinFromLat + sinDistance * cosFromLat * Math.Cos(heading);
            double dLng = Math.Atan2(
                    sinDistance * cosFromLat * Math.Sin(heading),
                    cosDistance - sinFromLat * sinLat);
            return new Position(Math.Asin(sinLat).ToDegrees(), (fromLng + dLng).ToDegrees());
        }
    }
}
