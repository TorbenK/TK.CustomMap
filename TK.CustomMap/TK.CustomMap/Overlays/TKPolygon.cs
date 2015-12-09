using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TK.CustomMap.Overlays
{
    /// <summary>
    /// A polygon to display on the map
    /// </summary>
    public class TKPolygon : TKBase
    {
        public const string CoordinatesPropertyName = "Coordinates";
        public const string FillColorPropertyName = "FillColor";
        public const string StrokeColorPropertyName = "StrokeColor";
        public const string StrokeWidthPropertyName = "StrokeWidth";

        private List<Position> _coordinates;
        private Color _fillColor;
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
        /// Gets/Sets the fill color of the polygon
        /// </summary>
        public Color FillColor
        {
            get { return this._fillColor; }
            set { this.SetField(ref this._fillColor, value); }
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
