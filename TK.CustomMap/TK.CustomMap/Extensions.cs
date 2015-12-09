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
            return string.Format("{0},{1}", self.Latitude, self.Longitude);
        }
    }
}
