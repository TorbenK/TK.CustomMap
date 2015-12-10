using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TK.CustomMap.Overlays
{
    /// <summary>
    /// A route to display on the map
    /// </summary>
    public class TKRoute : TKOverlay
    {
        public const string RouteCoordinatesPropertyName = "RouteCoordinates";
        public const string LineWidthProperty = "LineWidth";

        private List<Position> _routeCoordinates;
        private float _lineWidth;

        /// <summary>
        /// Coordinates of the route
        /// </summary>
        public List<Position> RouteCoordinates
        {
            get { return this._routeCoordinates; }
            set { this.SetField(ref this._routeCoordinates, value); }
        }
        /// <summary>
        /// Gets/Sets the width of the line
        /// </summary>
        public float LineWidth
        {
            get { return this._lineWidth; }
            set { this.SetField(ref this._lineWidth, value); }
        }
        /// <summary>
        /// Creates a new instance of <see cref="TKRoute"/>
        /// </summary>
        public TKRoute()
        {
            this._routeCoordinates = new List<Position>();
        }
    }
}
