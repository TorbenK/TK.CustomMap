using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TK.CustomMap.Api.Google
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
