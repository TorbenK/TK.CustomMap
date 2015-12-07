using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TK.CustomMap.Api.OSM
{
    public class OsmNominatimResult : IPlaceResult
    {
        [JsonProperty("place_id")]
        public string PlaceId { get; set; }
        [JsonProperty("osm_id")]
        public int OsmId { get; set; }
        [JsonProperty("lat")]
        public double Latitude { get; set; }
        [JsonProperty("lon")]
        public double Longitude { get; set; }
        [JsonProperty("display_name")]
        public string Description { get; set; }
        [JsonIgnore]
        public string SearchTerm { get; set; }
    }
}
