using Newtonsoft.Json;

namespace TK.CustomMap.Api.Google
{
    /// <summary>
    /// Prediction result of the Google Place API call
    /// </summary>
    public class GmsPlacePrediction : IPlaceResult
    {
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("place_id")]
        public string PlaceId { get; set; }
        [JsonProperty("reference")]
        public string Reference { get; set; }
    }
}
