using Newtonsoft.Json;

namespace TK.CustomMap.Api.Google
{
    /// <summary>
    /// A step inside a leg
    /// </summary>
    public class GmsDirectionResultLegStep
    {
        [JsonProperty("start_location")]
        public GmsLocation StartLocation { get; set; }
        [JsonProperty("end_location")]
        public GmsLocation EndLocation { get; set; }
        public GmsTextValue Distance { get; set; }
        public GmsTextValue Duration { get; set; }
        public GmsPolyline Polyline { get; set; }
        [JsonProperty("html_instructions")]
        public string HtmlInstructions { get; set; }
    }
}
