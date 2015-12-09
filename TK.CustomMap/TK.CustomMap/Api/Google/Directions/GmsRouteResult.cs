using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TK.CustomMap.Api.Google
{
    /// <summary>
    /// A route within the direction result
    /// </summary>
    public class GmsRouteResult
    {
        /// <summary>
        /// Gets the summary text
        /// </summary>
        public string Summary { get; set; }
        /// <summary>
        /// Gets the polyline
        /// </summary>
        [JsonProperty("overview_polyline")]
        public GmsPolyline Polyline { get; set; }
        /// <summary>
        /// Get the bounds of the route
        /// </summary>
        public GmsDirectionResultBounds Bounds { get; set; }
        /// <summary>
        /// Gets the legs of the routes
        /// </summary>
        public IEnumerable<GmsDirectionResultLeg> Legs { get; set; }
    }
}
