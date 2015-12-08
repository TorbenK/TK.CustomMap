using Newtonsoft.Json;
using System.Collections.Generic;

namespace TK.CustomMap.Api.Google
{
    /// <summary>
    /// Result class of the Google Place API call
    /// </summary>
    public class GmsPlaceResult
    {
        /// <summary>
        /// Predictions received by the Google Place API call
        /// </summary>
        [JsonProperty("predictions")]
        public IEnumerable<GmsPlacePrediction> Predictions { get; set; }
        /// <summary>
        /// The search term send to the Google Place API
        /// </summary>
        [JsonIgnore]
        public string SearchTerm { get; internal set; }
    }
}
