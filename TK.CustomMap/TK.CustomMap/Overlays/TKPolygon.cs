using System.Collections.Generic;
using Xamarin.Forms;

namespace TK.CustomMap.Overlays
{
    /// <summary>
    /// A polygon to display on the map
    /// </summary>
    public class TKPolygon : TKOverlay
    {
        public const string CoordinatesPropertyName = "Coordinates";
        public const string StrokeColorPropertyName = "StrokeColor";
        public const string StrokeWidthPropertyName = "StrokeWidth";

        private List<Position> _coordinates;
        private Color _strokeColor;
        private float _strokeWidth;
        /// <summary>
        /// List of positions of the polygon
        /// </summary>
        public List<Position> Coordinates
        {
            get { return this._coordinates; }
            set { this.SetField(ref this._coordinates, value); }
        }
        /// <summary>
        /// Gets/Sets the stroke color of the polygon
        /// </summary>
        public Color StrokeColor
        {
            get { return this._strokeColor; }
            set { this.SetField(ref this._strokeColor, value); }
        }
        /// <summary>
        /// Gets/Sets the width of the stroke
        /// </summary>
        public float StrokeWidth
        {
            get { return this._strokeWidth; }
            set { this.SetField(ref this._strokeWidth, value); }
        }
        /// <summary>
        /// Creates a new instance of <c>TKPolygon</c>
        /// </summary>
        public TKPolygon()
        {
            this._coordinates = new List<Position>();
        }
    }
}
