using System.Collections.Generic;
using Newtonsoft.Json;

namespace TK.CustomMap.Api.Google
{
    /// <summary>
    /// Result class of the Google Maps Directions API
    /// </summary>
    public class GmsDirectionResult
    {
        /// <summary>
        /// Status as text
        /// </summary>
        [JsonProperty("status")]
        private string StatusText { get; set; }
        /// <summary>
        /// Gets the result status of the API call
        /// </summary>
        public GmsDirectionResultStatus Status 
        {
            get
            {
                switch (this.StatusText)
                {
                    case "OK": return GmsDirectionResultStatus.Ok;
                    case "NOT_FOUND": return GmsDirectionResultStatus.NotFound;
                    case "ZERO_RESULTS": return GmsDirectionResultStatus.ZeroResults;
                    case "MAX_WAYPOINTS_EXCEEDED": return GmsDirectionResultStatus.MaxWaypointsExceeded;
                    case "INVALID_REQUEST": return GmsDirectionResultStatus.InvalidRequest;
                    case "OVER_QUERY_LIMIT": return GmsDirectionResultStatus.OverQueryLimit;
                    case "REQUEST_DENIED": return GmsDirectionResultStatus.RequestDenied;
                    case "UNKNOWN_ERROR": return GmsDirectionResultStatus.UnknownError;
                    default: return GmsDirectionResultStatus.UnknownError;
                }
            }
        }
        /// <summary>
        /// Gets the Route result
        /// </summary>
        public IEnumerable<GmsRouteResult> Routes { get; set; }
    }
}
