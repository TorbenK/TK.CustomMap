namespace TK.CustomMap.Overlays
{
    /// <summary>
    /// Internally used functions to set read-only properties on the <see cref="TKRoute"/>
    /// </summary>
    public interface IRouteFunctions
    {
        /// <summary>
        /// Sets the <value>Bounds</value>
        /// </summary>
        /// <param name="bounds">Boundary of the route</param>
        void SetBounds(MapSpan bounds);
        /// <summary>
        /// Sets the <value>Steps</value>
        /// </summary>
        /// <param name="steps">Steps of the route</param>
        void SetSteps(TKRouteStep[] steps);
        /// <summary>
        /// Sets the <value>TravelTime</value>
        /// </summary>
        /// <param name="travelTime">The travel time of the route</param>
        void SetTravelTime(double travelTime);
        /// <summary>
        /// Sets the <value>Distance</value>
        /// </summary>
        /// <param name="distance">The distance of the route</param>
        void SetDistance(double distance);
        /// <summary>
        /// Sets the <value>IsCalculated</value>
        /// </summary>
        /// <param name="calculated">Calculated</param>
        void SetIsCalculated(bool calculated);
    }
}
