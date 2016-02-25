using Xamarin.Forms.Maps;

namespace TK.CustomMap.Overlays
{
    /// <summary>
    /// A route to display on the map
    /// </summary>
    public class TKRoute : TKOverlay, IRouteFunctions
    {
        public const string SourceProperty = "Source";
        public const string DestinationProperty = "Destination";
        public const string LineWidthProperty = "LineWidth";
        public const string SelectableProperty = "Selectable";
        public const string TravelModelProperty = "TravelMode";
        public const string BoundsProperty = "Bounds";
        public const string StepsProperty = "Steps";
        public const string DistanceProperty = "Distance";
        public const string TravelTimeProperty = "TravelTime";
        public const string IsCalculatedProperty = "IsCalculated";

        private Position _source;
        private Position _destination;
        private float _lineWidth;
        private bool _selectAble;
        private TKRouteTravelMode _travelMode;
        private MapSpan _bounds;
        private TKRouteStep[] _steps;
        private double _distance;
        private double _travelTime;
        private bool _isCalculated;
        /// <summary>
        /// Gets/Sets the source of the route
        /// </summary>
        public Position Source
        {
            get { return this._source; }
            set { this.SetField(ref this._source, value); }
        }
        /// <summary>
        /// Gets/Sets the destination of the route
        /// </summary>
        public Position Destination
        {
            get { return this._destination; }
            set { this.SetField(ref this._destination, value); }
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
        /// Gets/Sets if a route is selectable. If this is <value>false</value> <see cref="TK.CustomMap.TKCustomMap.RouteClickedCommand"/> will not get raised for this route
        /// </summary>
        public bool Selectable
        {
            get { return this._selectAble; }
            set { this.SetField(ref this._selectAble, value); }
        }
        /// <summary>
        /// Gets/Sets the travel mode for a route calculation.
        /// </summary>
        public TKRouteTravelMode TravelMode
        {
            get { return this._travelMode; }
            set { this.SetField(ref this._travelMode, value); }
        }
        /// <summary>
        /// Gets the bounds of the route. This is set automatically by the renderer during route calculation.
        /// </summary>
        public MapSpan Bounds
        {
            get { return this._bounds; }
            private set { this.SetField(ref this._bounds, value); }
        }
        /// <summary>
        /// Gets the steps of the route
        /// </summary>
        public TKRouteStep[] Steps
        {
            get { return this._steps; }
            private set { this.SetField(ref this._steps, value); }
        }
        /// <summary>
        /// Gets the distance of the route in meters
        /// </summary>
        public double Distance
        {
            get { return this._distance; }
            private set { this.SetField(ref this._distance, value); }
        }
        /// <summary>
        /// Gets the travel time of the route in seconds
        /// </summary>
        public double TravelTime
        {
            get { return this._travelTime; }
            private set { this.SetField(ref this._travelTime, value); }
        }
        public bool IsCalculated
        {
            get { return this._isCalculated; }
            private set { this.SetField(ref this._isCalculated, value); }
        }
        /// <summary>
        /// Creates a new instance of <see cref="TKRoute"/>
        /// </summary>
        public TKRoute()
        {
            this.LineWidth = 2.5f;
            this.Selectable = true;
            this.TravelMode = TKRouteTravelMode.Driving;
        }
        ///<inheritdoc/>
        void IRouteFunctions.SetBounds(MapSpan bounds)
        {
            this.Bounds = bounds;
        }
        ///<inheritdoc/>
        void IRouteFunctions.SetSteps(TKRouteStep[] steps)
        {
            this.Steps = steps;
        }
        ///<inheritdoc/>
        void IRouteFunctions.SetTravelTime(double travelTime)
        {
            this.TravelTime = travelTime;   
        }
        ///<inheritdoc/>
        void IRouteFunctions.SetDistance(double distance)
        {
            this.Distance = distance;
        }
        ///<inheritdoc/>
        void IRouteFunctions.SetIsCalculated(bool calculated)
        {
            this.IsCalculated = calculated;
        }
    }
}
