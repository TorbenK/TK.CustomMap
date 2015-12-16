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

        private Position _source;
        private Position _destination;
        private float _lineWidth;
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
    }
}
