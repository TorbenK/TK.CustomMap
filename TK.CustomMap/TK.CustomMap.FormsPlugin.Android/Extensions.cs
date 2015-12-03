using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Maps;
using Android.Gms.Maps.Model;

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
    }
}