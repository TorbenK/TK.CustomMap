namespace TK.CustomMap.Api.Google
{
    /// <summary>
    /// The status returned by the Google places API
    /// </summary>
    public enum GmsDetailsResultStatus
    {
        Ok,
        UnknownError,
        ZeroResults,
        OverQueryLimit,
        RequestDenied,
        InvalidRequest,
        NotFound,
        UnknownStatus
    }
}
