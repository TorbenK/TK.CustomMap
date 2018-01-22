using Newtonsoft.Json;

namespace TK.CustomMap.Api.Google
{
    /// <summary>
    /// Result of the Google Maps Places details request
    /// </summary>
    public class GmsDetailsResult
    {
        [JsonProperty("status")]
         string StatusText { get; set; }
        /// <summary>
        /// Status of the API call
        /// </summary>
        public GmsDetailsResultStatus Status
        {
            get
            {
                switch (StatusText)
                {
                    case "OK":
                        return GmsDetailsResultStatus.Ok;
                    case "UNKNOWN_ERROR":
                        return GmsDetailsResultStatus.UnknownError;
                    case "ZERO_RESULTS":
                        return GmsDetailsResultStatus.ZeroResults;
                    case "OVER_QUERY_LIMIT":
                        return GmsDetailsResultStatus.OverQueryLimit;
                    case "REQUEST_DENIED":
                        return GmsDetailsResultStatus.RequestDenied;
                    case "INVALID_REQUEST":
                        return GmsDetailsResultStatus.InvalidRequest;
                    case "NOT_FOUND":
                        return GmsDetailsResultStatus.NotFound;
                    default:
                        return GmsDetailsResultStatus.UnknownStatus;
                }
            }
        }
        /// <summary>
        /// Result item
        /// </summary>
        [JsonProperty("result")]
        public GmsDetailsResultItem Item { get; set; }
    }
}
