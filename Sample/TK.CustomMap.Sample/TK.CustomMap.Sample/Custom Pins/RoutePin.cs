using TK.CustomMap.Overlays;

namespace TK.CustomMap.Sample
{
    public class RoutePin : TKCustomMapPin
    {
        /// <summary>
        /// Gets/Sets if the pin is the source or destination of the route
        /// </summary>
        public bool IsSource { get; set; }
        /// <summary>
        /// Gets/Sets a reference to the route
        /// </summary>
        public TKRoute Route { get; set; }
    }
}
