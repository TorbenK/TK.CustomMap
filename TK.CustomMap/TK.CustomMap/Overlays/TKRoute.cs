using System.Collections.Generic;
using Xamarin.Forms.Maps;

namespace TK.CustomMap.Overlays
{
    /// <summary>
    /// A route to display on the map
    /// </summary>
    public class TKRoute : TKOverlay
    {
        public const string SourceProperty = "Source";
        public const string DestinationProperty = "Destination";
        public const string LineWidthProperty = "LineWidth";
        public const string SelectableProperty = "Selectable";
        public const string TravelModelProperty = "TravelMode";

        private Position _source;
        private Position _destination;
        private float _lineWidth;
        private bool _selectAble;
        private TKRouteTravelMode _travelMode;
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
        /// Creates a new instance of <see cref="TKRoute"/>
        /// </summary>
        public TKRoute()
        {
            this.LineWidth = 2.5f;
            this.Selectable = true;
            this.TravelMode = TKRouteTravelMode.Driving;
        }
    }
}
