namespace TK.CustomMap.Overlays
{
    /// <summary>
    /// Internally used functions to set read-only properties on <see cref="TKRouteStep"/>
    /// </summary>
    public interface IRouteStepFunctions
    {
        /// <summary>
        /// Sets the distance
        /// </summary>
        /// <param name="distance">The distance</param>
        void SetDistance(double distance);
        /// <summary>
        /// Sets the instructions
        /// </summary>
        /// <param name="instructions">The instructions</param>
        void SetInstructions(string instructions);
    }
}
