using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TK.CustomMap.Api.OSM
{
    /// <summary>
    /// Result class of the OSM Nominatim search API call
    /// </summary>
    public class OsmNominatimResult : IPlaceResult
    {
        /// <summary>
        /// Gets/Sets the id of the place
        /// </summary>
        [JsonProperty("place_id")]
        public string PlaceId { get; set; }
        /// <summary>
        /// Gets/Sets the OSM id
        /// </summary>
        [JsonProperty("osm_id")]
        public string OsmId { get; set; }
        /// <summary>
        /// Gets/Sets latitude
        /// </summary>
        [JsonProperty("lat")]
        public double Latitude { get; set; }
        /// <summary>
        /// Gets/Sets longitude
        /// </summary>
        [JsonProperty("lon")]
        public double Longitude { get; set; }
        /// <summary>
        /// Gets/Sets description
        /// </summary>
        [JsonProperty("display_name")]
        public string Description { get; set; }
    }
}
