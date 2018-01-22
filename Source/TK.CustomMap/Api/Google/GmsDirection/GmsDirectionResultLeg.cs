using System.Collections.Generic;
using Newtonsoft.Json;

namespace TK.CustomMap.Api.Google
{
    /// <summary>
    /// A leg of the Google Maps Directions API result
    /// </summary>
    public class GmsDirectionResultLeg
    {
        /// <summary>
        /// Gets the start address of the leg
        /// </summary>
        [JsonProperty("start_address")]
        public string StartAddress { get; set; }
        /// <summary>
        /// Gets the end address of the leg
        /// </summary>
        [JsonProperty("end_address")]
        public string EndAddress { get; set; }
        /// <summary>
        /// Gets the start location of the leg
        /// </summary>
        [JsonProperty("start_location")]
        public GmsLocation StartLocation { get; set; }
        /// <summary>
        /// Gets the end location of the leg
        /// </summary>
        [JsonProperty("end_location")]
        public GmsLocation EndLocation { get; set; }
        /// <summary>
        /// Gets the distance of the leg
        /// </summary>
        public GmsTextValue Distance { get; set; }
        /// <summary>
        /// Gets the duration of the leg
        /// </summary>
        public GmsTextValue Duration { get; set; }
        /// <summary>
        /// Gets the steps of the leg
        /// </summary>
        public IEnumerable<GmsDirectionResultLegStep> Steps { get; set; }
    }
}
