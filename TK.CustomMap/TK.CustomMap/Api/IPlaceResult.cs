namespace TK.CustomMap.Api
{
    /// <summary>
    /// Base interface for result class
    /// </summary>
    public interface IPlaceResult
    {
        /// <summary>
        /// Description of the place
        /// </summary>
        string Description { get; set; }
        /// <summary>
        /// Gets/Sets the subtitle of the place. This is only returned by the <value>MKLocalSearch</value> on iOS
        /// </summary>
        string Subtitle { get; set; }
    }
}
