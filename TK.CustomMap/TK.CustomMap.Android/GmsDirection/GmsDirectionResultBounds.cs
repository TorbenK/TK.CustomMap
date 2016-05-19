using Newtonsoft.Json;

namespace TK.CustomMap.Droid.Api
{
    /// <summary>
    /// Bounds of the Direction API call result
    /// </summary>
    public class GmsDirectionResultBounds
    {
        /// <summary>
        /// Gets the north-east boundary
        /// </summary>
        [JsonProperty("northeast")]
        public GmsLocation NorthEast { get; set; }
        /// <summary>
        /// Gets the south-west boundary
        /// </summary>
        [JsonProperty("southwest")]
        public GmsLocation SouthWest { get; set; }
    }
}
