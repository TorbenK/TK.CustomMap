using Newtonsoft.Json;
using Xamarin.Forms.Maps;

namespace TK.CustomMap.Api.Google
{
    /// <summary>
    /// Holds latitude longitude
    /// </summary>
    public struct GmsLocation
    {
        /// <summary>
        /// Latitude
        /// </summary>
        [JsonProperty("lat")]
        public double Latitude { get; set; }
        /// <summary>
        /// Longitude
        /// </summary>
        [JsonProperty("Lng")]
        public double Longitude { get; set; }
        /// <summary>
        /// Convert to position
        /// </summary>
        /// <returns><see cref="Position"/></returns>
        public Position ToPosition()
        {
            return new Position(this.Latitude, this.Longitude);
        }
    }
}
