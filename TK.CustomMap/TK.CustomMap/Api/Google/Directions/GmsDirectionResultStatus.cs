using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TK.CustomMap.Api.Google
{
    /// <summary>
    /// Result status of the Google Maps Directions API call
    /// </summary>
    public enum GmsDirectionResultStatus
    {
        Ok,
        NotFound,
        ZeroResults,
        MaxWaypointsExceeded,
        InvalidRequest,
        OverQueryLimit,
        RequestDenied,
        UnknownError
    }
}
