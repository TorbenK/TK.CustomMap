using System.Collections.Generic;

namespace TK.CustomMap.Overlays
{
    public class TKPolyline : TKOverlay
    {
        public const string LineCoordinatesPropertyName = "LineCoordinates";
        public const string LineWidthProperty = "LineWidth";

         List<Position> _lineCoordinates;
         float _lineWidth;

        /// <summary>
        /// Coordinates of the line
        /// </summary>
        public List<Position> LineCoordinates
        {
            get { return _lineCoordinates; }
            set { SetField(ref _lineCoordinates, value); }
        }
        /// <summary>
        /// Gets/Sets the width of the line
        /// </summary>
        public float LineWidth
        {
            get { return _lineWidth; }
            set { SetField(ref _lineWidth, value); }
        }
        /// <summary>
        /// Creates a new instance of <see cref="TKPolyline"/>
        /// </summary>
        public TKPolyline()
        {
            _lineCoordinates = new List<Position>();
        }
    }
}
