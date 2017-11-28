using System.Collections.Generic;
using Xamarin.Forms;

namespace TK.CustomMap.Overlays
{
    /// <summary>
    /// A polygon to display on the map
    /// </summary>
    public class TKPolygon : TKOverlay
    {
         List<Position> _coordinates;
         Color _strokeColor;
         float _strokeWidth;
        /// <summary>
        /// List of positions of the polygon
        /// </summary>
        public List<Position> Coordinates
        {
            get { return _coordinates; }
            set { SetField(ref _coordinates, value); }
        }
        /// <summary>
        /// Gets/Sets the stroke color of the polygon
        /// </summary>
        public Color StrokeColor
        {
            get { return _strokeColor; }
            set { SetField(ref _strokeColor, value); }
        }
        /// <summary>
        /// Gets/Sets the width of the stroke
        /// </summary>
        public float StrokeWidth
        {
            get { return _strokeWidth; }
            set { SetField(ref _strokeWidth, value); }
        }
        /// <summary>
        /// Creates a new instance of <c>TKPolygon</c>
        /// </summary>
        public TKPolygon()
        {
            _coordinates = new List<Position>();
        }
    }
}
